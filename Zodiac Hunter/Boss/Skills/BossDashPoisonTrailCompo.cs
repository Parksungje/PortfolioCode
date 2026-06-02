using System.Collections.Generic;
using Code.Combat;
using Code.Entities;
using Code.StatusEffectSystem;
using UnityEngine;

namespace Work.PSJ.Code.Boss.Skills
{
    public class BossDashPoisonTrailCompo : BossSkillCompo
    {
        [SerializeField] private float trailPointSpacing = 0.35f;
        [SerializeField] private float cornerSmoothDistance = 0.45f;
        [SerializeField] private int cornerSmoothSteps = 4;
        [SerializeField] private float poisonRadius = 1.5f;
        [SerializeField] private float poisonDuration = 4f;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private float damageInterval = 0.5f;
        [SerializeField] private float poisonDamage = 6f;
        [SerializeField] private LayerMask poisonTargetLayer = -1;
        [SerializeField] private Material poisonMaterial;
        [SerializeField] private List<StatusEffectApplyData> poisonEffects = new List<StatusEffectApplyData>();

        private const string PoisonSortingLayerName = "Ground";
        private const int PoisonSortOrder = 2;
        private const int CapSegments = 8;
        private const float StraightDot = 0.995f;
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly Color PoisonColor = new Color(0.18f, 0.75f, 0.08f, 0.55f);

        private class PoisonTrail
        {
            public readonly List<Vector2> damagePoints = new List<Vector2>();
            public GameObject obj;
            public Mesh mesh;
            public MeshRenderer renderer;
            public Color[] colors;
            public float activeUntil;
            public float destroyAt;
            public float alpha = 1f;
        }

        private readonly List<PoisonTrail> _trails = new List<PoisonTrail>();
        private readonly List<Vector2> _renderPoints = new List<Vector2>();
        private readonly Collider2D[] _hits = new Collider2D[16];
        private readonly HashSet<EntityHealthModule> _damagedTargets = new HashSet<EntityHealthModule>();

        private ContactFilter2D _contactFilter;
        private PoisonTrail _currentTrail;
        private bool _isTracing;
        private bool _hasLastPoint;
        private Vector2 _lastPoint;
        private float _nextDamageTime;
        private MaterialPropertyBlock _poisonBlock;

        protected override void OnInitializeSkill(Entity entity)
        {
            _contactFilter = new ContactFilter2D();
            _contactFilter.SetLayerMask(poisonTargetLayer);
            _contactFilter.useTriggers = true;
        }

        private void Update()
        {
            if (_isTracing)
                AddTrailPoint(transform.position);

            UpdateTrailFades();
            RemoveExpiredTrails();

            if (_trails.Count == 0 || Time.time < _nextDamageTime)
                return;

            ApplyPoison();
            _nextDamageTime = Time.time + Mathf.Max(0.05f, damageInterval);
        }

        public void BeginTrail()
        {
            if (!CanUseSkill)
                return;

            EndTrail();

            _currentTrail = CreateTrail();
            _trails.Add(_currentTrail);
            _isTracing = true;
            _hasLastPoint = false;

            AddTrailPoint(transform.position, true);

            if (_trails.Count == 1)
                _nextDamageTime = Time.time;
        }

        public void EndTrail()
        {
            if (!_isTracing)
                return;

            AddTrailPoint(transform.position, true);

            _isTracing = false;
            _hasLastPoint = false;

            if (_currentTrail != null)
            {
                _currentTrail.activeUntil = Time.time + poisonDuration;
                _currentTrail.destroyAt = _currentTrail.activeUntil + Mathf.Max(0f, fadeDuration);
            }

            _currentTrail = null;
        }

        public void SpawnPoisonAtCurrentPosition()
        {
            BeginTrail();
            EndTrail();
        }

        private PoisonTrail CreateTrail()
        {
            GameObject obj = new GameObject("DashPoisonTrail");
            Mesh mesh = new Mesh { name = "DashPoisonTrailMesh" };

            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
            renderer.sortingLayerName = PoisonSortingLayerName;
            renderer.sortingOrder = PoisonSortOrder;
            if (poisonMaterial != null)
                renderer.sharedMaterial = poisonMaterial;

            return new PoisonTrail
            {
                obj = obj,
                mesh = mesh,
                renderer = renderer,
                activeUntil = float.PositiveInfinity,
                destroyAt = float.PositiveInfinity
            };
        }

        private void AddTrailPoint(Vector2 point, bool force = false)
        {
            if (_currentTrail == null)
                return;

            if (!_hasLastPoint)
            {
                AddPointToTrail(point);
                _lastPoint = point;
                _hasLastPoint = true;
                return;
            }

            float spacing = Mathf.Max(0.05f, trailPointSpacing);
            Vector2 delta = point - _lastPoint;
            float distance = delta.magnitude;

            if (!force && distance < spacing)
                return;
            if (distance <= 0.001f)
                return;

            while (distance >= spacing)
            {
                Vector2 nextPoint = _lastPoint + delta.normalized * spacing;
                AddPointToTrail(nextPoint);
                _lastPoint = nextPoint;

                delta = point - _lastPoint;
                distance = delta.magnitude;
            }

            if (force && distance > 0.05f)
            {
                AddPointToTrail(point);
                _lastPoint = point;
            }
        }

        private void AddPointToTrail(Vector2 point)
        {
            _currentTrail.damagePoints.Add(point);
            RebuildTrailMesh(_currentTrail);
        }

        private void RebuildTrailMesh(PoisonTrail trail)
        {
            int pointCount = trail.damagePoints.Count;
            if (pointCount <= 0)
            {
                trail.mesh.Clear();
                return;
            }

            if (pointCount == 1)
            {
                BuildCircleMesh(trail, trail.damagePoints[0]);
                return;
            }

            BuildRenderPoints(trail.damagePoints);
            BuildPathMesh(trail, _renderPoints);
        }

        private void BuildRenderPoints(List<Vector2> points)
        {
            _renderPoints.Clear();
            AddRenderPoint(points[0]);

            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector2 prev = points[i - 1];
                Vector2 current = points[i];
                Vector2 next = points[i + 1];

                Vector2 inDir = (current - prev).normalized;
                Vector2 outDir = (next - current).normalized;
                if (inDir.sqrMagnitude <= 0.0001f || outDir.sqrMagnitude <= 0.0001f)
                    continue;

                if (Vector2.Dot(inDir, outDir) > StraightDot)
                    continue;

                float smoothDistance = Mathf.Min(
                    Mathf.Max(0.01f, cornerSmoothDistance),
                    Vector2.Distance(prev, current) * 0.5f,
                    Vector2.Distance(current, next) * 0.5f);

                Vector2 start = current - inDir * smoothDistance;
                Vector2 end = current + outDir * smoothDistance;
                AddRenderPoint(start);

                int steps = Mathf.Max(1, cornerSmoothSteps);
                for (int step = 1; step <= steps; step++)
                {
                    float t = step / (float)(steps + 1);
                    AddRenderPoint(Bezier(start, current, end, t));
                }

                AddRenderPoint(end);
            }

            AddRenderPoint(points[points.Count - 1]);
        }

        private void AddRenderPoint(Vector2 point)
        {
            if (_renderPoints.Count > 0 && Vector2.Distance(_renderPoints[_renderPoints.Count - 1], point) < 0.02f)
                return;

            _renderPoints.Add(point);
        }

        private Vector2 Bezier(Vector2 start, Vector2 control, Vector2 end, float t)
        {
            float inv = 1f - t;
            return inv * inv * start + 2f * inv * t * control + t * t * end;
        }

        private void BuildPathMesh(PoisonTrail trail, List<Vector2> points)
        {
            Mesh mesh = trail.mesh;
            int pointCount = points.Count;
            int stripVertexCount = pointCount * 2;
            int capVertexCount = (CapSegments + 2) * 2;
            int stripTriangleCount = (pointCount - 1) * 6;
            int capTriangleCount = CapSegments * 3 * 2;

            Vector3[] vertices = new Vector3[stripVertexCount + capVertexCount];
            Color[] colors = new Color[vertices.Length];
            int[] triangles = new int[stripTriangleCount + capTriangleCount];

            for (int i = 0; i < pointCount; i++)
            {
                Vector2 direction = GetDirection(points, i);
                Vector2 normal = new Vector2(-direction.y, direction.x) * poisonRadius;
                vertices[i * 2] = points[i] + normal;
                vertices[i * 2 + 1] = points[i] - normal;
            }

            int triangleIndex = 0;
            for (int i = 0; i < pointCount - 1; i++)
            {
                int left = i * 2;
                int right = left + 1;
                int nextLeft = left + 2;
                int nextRight = left + 3;

                triangles[triangleIndex++] = left;
                triangles[triangleIndex++] = right;
                triangles[triangleIndex++] = nextLeft;
                triangles[triangleIndex++] = nextLeft;
                triangles[triangleIndex++] = right;
                triangles[triangleIndex++] = nextRight;
            }

            int vertexIndex = stripVertexCount;
            AddCap(vertices, triangles, ref vertexIndex, ref triangleIndex, points[0], GetDirection(points, 0), true);
            AddCap(vertices, triangles, ref vertexIndex, ref triangleIndex, points[pointCount - 1], GetDirection(points, pointCount - 1), false);

            mesh.Clear();
            mesh.vertices = vertices;
            trail.colors = colors;
            ApplyTrailColor(trail, trail.alpha, true);
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
        }

        private void BuildCircleMesh(PoisonTrail trail, Vector2 center)
        {
            Mesh mesh = trail.mesh;
            int outerVertexCount = CapSegments * 2;
            int vertexCount = outerVertexCount + 1;
            Vector3[] vertices = new Vector3[vertexCount];
            Color[] colors = new Color[vertexCount];
            int[] triangles = new int[outerVertexCount * 3];

            vertices[0] = center;
            for (int i = 0; i < outerVertexCount; i++)
            {
                float angle = i / (float)outerVertexCount * Mathf.PI * 2f;
                vertices[i + 1] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * poisonRadius;
            }

            int triangleIndex = 0;
            for (int i = 0; i < outerVertexCount; i++)
            {
                triangles[triangleIndex++] = 0;
                triangles[triangleIndex++] = i + 1;
                triangles[triangleIndex++] = i == outerVertexCount - 1 ? 1 : i + 2;
            }

            mesh.Clear();
            mesh.vertices = vertices;
            trail.colors = colors;
            ApplyTrailColor(trail, trail.alpha, true);
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
        }

        private void AddCap(Vector3[] vertices, int[] triangles, ref int vertexIndex, ref int triangleIndex, Vector2 center, Vector2 direction, bool isStart)
        {
            Vector2 normal = new Vector2(-direction.y, direction.x);
            Vector2 capDirection = isStart ? -direction : direction;
            float startAngle = isStart ? -Mathf.PI * 0.5f : Mathf.PI * 0.5f;
            float endAngle = isStart ? Mathf.PI * 0.5f : -Mathf.PI * 0.5f;

            int centerIndex = vertexIndex++;
            vertices[centerIndex] = center;
            int firstArcIndex = vertexIndex;

            for (int i = 0; i <= CapSegments; i++)
            {
                float t = i / (float)CapSegments;
                float angle = Mathf.Lerp(startAngle, endAngle, t);
                Vector2 offset = (capDirection * Mathf.Cos(angle) + normal * Mathf.Sin(angle)) * poisonRadius;
                vertices[vertexIndex++] = center + offset;
            }

            for (int i = 0; i < CapSegments; i++)
            {
                triangles[triangleIndex++] = centerIndex;
                triangles[triangleIndex++] = firstArcIndex + i + 1;
                triangles[triangleIndex++] = firstArcIndex + i;
            }
        }

        private Vector2 GetDirection(List<Vector2> points, int index)
        {
            Vector2 direction;
            if (index == 0)
                direction = points[1] - points[0];
            else if (index == points.Count - 1)
                direction = points[index] - points[index - 1];
            else
                direction = points[index + 1] - points[index - 1];

            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        }

        private void ApplyPoison()
        {
            _damagedTargets.Clear();

            DamageData damage = CombatCalculator != null
                ? CombatCalculator.CalculateDamage(poisonDamage)
                : new DamageData { Damage = poisonDamage };

            for (int trailIndex = 0; trailIndex < _trails.Count; trailIndex++)
            {
                if (Time.time >= _trails[trailIndex].activeUntil)
                    continue;

                List<Vector2> points = _trails[trailIndex].damagePoints;
                for (int pointIndex = 0; pointIndex < points.Count; pointIndex++)
                    ApplyPoisonAtPoint(points[pointIndex], damage);
            }
        }

        private void ApplyPoisonAtPoint(Vector2 center, DamageData damage)
        {
            int hitCount = Physics2D.OverlapCircle(center, poisonRadius, _contactFilter, _hits);
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = _hits[i];
                if (hit == null)
                    continue;
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    continue;
                if (!hit.TryGetComponent(out EntityHealthModule health))
                    continue;
                if (!_damagedTargets.Add(health))
                    continue;

                health.ApplyDamage(damage, hit.ClosestPoint(center), OwnerEntity);
                ApplyPoisonEffects(hit, health);
            }
        }

        private void ApplyPoisonEffects(Collider2D hit, EntityHealthModule health)
        {
            if (poisonEffects == null || poisonEffects.Count == 0)
                return;

            IStatusEffectable effectable = hit.GetComponent<IStatusEffectable>();
            if (effectable == null)
                effectable = health.GetComponent<IStatusEffectable>();
            if (effectable == null)
                return;

            for (int i = 0; i < poisonEffects.Count; i++)
            {
                StatusEffectApplyData applyData = poisonEffects[i];
                if (applyData.EffectSO != null)
                    effectable.ApplyStatusEffect(applyData, OwnerEntity);
            }
        }

        public override void StopSkill()
        {
            _isTracing = false;
            _hasLastPoint = false;
            _currentTrail = null;
            ClearTrails();
        }

        private void RemoveExpiredTrails()
        {
            for (int i = _trails.Count - 1; i >= 0; i--)
            {
                if (Time.time < _trails[i].destroyAt)
                    continue;

                DestroyTrail(_trails[i]);
                _trails.RemoveAt(i);
            }
        }

        private void ClearTrails()
        {
            for (int i = _trails.Count - 1; i >= 0; i--)
                DestroyTrail(_trails[i]);

            _trails.Clear();
        }

        private void DestroyTrail(PoisonTrail trail)
        {
            if (trail == null)
                return;

            if (trail.obj != null)
                Destroy(trail.obj);
            if (trail.mesh != null)
                Destroy(trail.mesh);
        }

        private void UpdateTrailFades()
        {
            for (int i = 0; i < _trails.Count; i++)
            {
                PoisonTrail trail = _trails[i];
                if (Time.time < trail.activeUntil)
                {
                    ApplyTrailColor(trail, 1f);
                    continue;
                }

                float fadeTime = Mathf.Max(0.01f, fadeDuration);
                float fadeProgress = Mathf.Clamp01((Time.time - trail.activeUntil) / fadeTime);
                ApplyTrailColor(trail, 1f - fadeProgress);
            }
        }

        private void ApplyTrailColor(PoisonTrail trail, float alpha, bool force = false)
        {
            alpha = Mathf.Clamp01(alpha);
            if (!force && Mathf.Approximately(trail.alpha, alpha))
                return;

            trail.alpha = alpha;
            Color color = PoisonColor;
            color.a *= alpha;

            if (trail.colors != null)
            {
                for (int i = 0; i < trail.colors.Length; i++)
                    trail.colors[i] = color;

                trail.mesh.colors = trail.colors;
            }

            if (trail.renderer == null)
                return;

            if (_poisonBlock == null)
                _poisonBlock = new MaterialPropertyBlock();

            _poisonBlock.SetColor(ColorId, color);
            trail.renderer.SetPropertyBlock(_poisonBlock);
        }

        private void OnDisable()
        {
            StopSkill();
        }

        private void OnDestroy()
        {
            StopSkill();
        }
    }
}

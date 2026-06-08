using System.Collections;
using System.Collections.Generic;
using Code.Combat;
using Code.Entities;
using Code.Player;
using UnityEngine;
using Work.PSJ.Code.Enemies;

namespace Work.PSJ.Code.Boss.Skills
{
    public class BossPoisonRainSkillCompo : BossSkillCompo
    {
        [SerializeField] private bool runOnlyWhileAttacking = true;
        [SerializeField] private float poisonDropInterval = 1.2f;
        [SerializeField] private int poisonDropsPerCycle = 2;
        [SerializeField] private float poisonDropSpread = 4f;
        [SerializeField] private float poisonImpactRadius = 1.4f;
        [SerializeField] private float poisonWarningDuration = 0.8f;
        [SerializeField] private float poisonDamage = 15f;
        [SerializeField] private LayerMask poisonTargetLayer = ~0;
        [SerializeField] private PlayerSO playerTarget;
        [SerializeField] private Material warningMaterial;
        [SerializeField] [Range(0.01f, 0.5f)] private float warningLineWidth = 0.08f;
        [SerializeField] [Range(0f, 1f)] private float warningFillStartScale = 0.15f;
        [SerializeField] [Range(0f, 1f)] private float warningFillStartAlpha = 0.08f;
        [SerializeField] [Range(0f, 1f)] private float warningFillEndAlpha = 0.32f;

        private const int WarningSegments = 48;
        private const int WarningSortOrder = 25;
        private const int FillSortOrder = WarningSortOrder - 1;
        private const float MinInterval = 0.01f;
        private static readonly Color WarningColor = new Color(0.18f, 0.75f, 0.08f, 0.65f);
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private BasePatternAttackCompo _attackCompo;
        private Material _runtimeWarningMaterial;
        private Coroutine _poisonRainCoroutine;
        private LineRenderer _warningLine;
        private MeshRenderer _warningFill;
        private Mesh _warningFillMesh;
        private MaterialPropertyBlock _warningFillBlock;

        private readonly Collider2D[] _poisonHits = new Collider2D[8];
        private readonly HashSet<EntityHealthModule> _damagedTargets = new HashSet<EntityHealthModule>();

        private bool CanRunSkill =>
            CanUseSkill && (!runOnlyWhileAttacking || (_attackCompo != null && _attackCompo.IsAttackActive));

        protected override void OnInitializeSkill(Entity entity)
        {
            _attackCompo = GetComponent<BasePatternAttackCompo>() ??
                           entity.GetComponentInChildren<BasePatternAttackCompo>();

            if (playerTarget == null)
                playerTarget = OwnerBoss?.Target;
        }

        private void Update()
        {
            if (!CanRunSkill)
            {
                if (_poisonRainCoroutine != null)
                    StopSkill();
                return;
            }

            if (_poisonRainCoroutine == null)
                _poisonRainCoroutine = StartCoroutine(PoisonRainLoop());
        }

        private IEnumerator PoisonRainLoop()
        {
            WaitForSeconds wait = new WaitForSeconds(Mathf.Max(MinInterval, poisonDropInterval));

            while (CanRunSkill)
            {
                int dropCount = Mathf.Max(1, poisonDropsPerCycle);
                for (int i = 0; i < dropCount && CanRunSkill; i++)
                    yield return DropPoisonOnce();
                if (!CanRunSkill)
                    break;

                yield return wait;
            }

            _poisonRainCoroutine = null;
        }

        private IEnumerator DropPoisonOnce()
        {
            Vector2 dropCenter = GetDropCenter();
            yield return ShowWarningCircle(dropCenter);
            if (!CanRunSkill)
                yield break;

            ApplyPoisonDamage(dropCenter);
        }

        private Vector2 GetDropCenter()
        {
            Transform target = GetTargetTransform();
            Vector2 basePoint = target != null ? (Vector2)target.position : (Vector2)transform.position;

            return basePoint + Random.insideUnitCircle * poisonDropSpread;
        }

        private Transform GetTargetTransform()
        {
            if (playerTarget != null && playerTarget.player != null)
                return playerTarget.player.transform;

            return _attackCompo != null ? _attackCompo.AttackTarget : null;
        }

        private IEnumerator ShowWarningCircle(Vector2 center)
        {
            if (poisonWarningDuration <= 0f)
                yield break;

            LineRenderer line = _warningLine != null ? _warningLine : CreateWarningLine();
            MeshRenderer fill = _warningFill != null ? _warningFill : CreateWarningFill();
            line.transform.position = center;
            fill.transform.position = center;
            line.enabled = true;
            fill.enabled = true;

            float elapsed = 0f;
            while (elapsed < poisonWarningDuration && CanRunSkill)
            {
                float progress = Mathf.Clamp01(elapsed / poisonWarningDuration);
                UpdateWarningProgress(line, fill, progress);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (CanRunSkill)
                UpdateWarningProgress(line, fill, 1f);

            line.enabled = false;
            fill.enabled = false;
        }

        private void UpdateWarningProgress(LineRenderer line, MeshRenderer fill, float progress)
        {
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            float fillScale = Mathf.Lerp(warningFillStartScale, 1f, easedProgress) * poisonImpactRadius;
            fill.transform.localScale = new Vector3(fillScale, fillScale, 1f);

            Color fillColor = WarningColor;
            fillColor.a = Mathf.Lerp(warningFillStartAlpha, warningFillEndAlpha, easedProgress);
            _warningFillBlock ??= new MaterialPropertyBlock();
            _warningFillBlock.SetColor(ColorId, fillColor);
            fill.SetPropertyBlock(_warningFillBlock);

            float urgency = Mathf.SmoothStep(0.65f, 1f, progress);
            float pulse = 1f + Mathf.Sin(progress * Mathf.PI * 10f) * 0.25f * urgency;
            float width = warningLineWidth * pulse;
            line.startWidth = width;
            line.endWidth = width;
        }

        private LineRenderer CreateWarningLine()
        {
            _warningLine = new GameObject("PoisonWarningCircle").AddComponent<LineRenderer>();
            _warningLine.useWorldSpace = false;
            _warningLine.loop = true;
            _warningLine.alignment = LineAlignment.View;
            _warningLine.textureMode = LineTextureMode.Stretch;
            _warningLine.numCornerVertices = 4;
            _warningLine.numCapVertices = 4;
            _warningLine.startWidth = warningLineWidth;
            _warningLine.endWidth = warningLineWidth;
            _warningLine.startColor = WarningColor;
            _warningLine.endColor = WarningColor;
            _warningLine.sortingOrder = WarningSortOrder;

            _warningLine.positionCount = WarningSegments;
            for (int i = 0; i < WarningSegments; i++)
            {
                float angle = i / (float)WarningSegments * Mathf.PI * 2f;
                _warningLine.SetPosition(i, new Vector3(Mathf.Cos(angle) * poisonImpactRadius, Mathf.Sin(angle) * poisonImpactRadius, 0f));
            }

            _warningLine.enabled = false;
            _warningLine.material = GetWarningMaterial();
            return _warningLine;
        }

        private MeshRenderer CreateWarningFill()
        {
            GameObject fillObject = new GameObject("PoisonWarningFill");
            fillObject.AddComponent<MeshFilter>().sharedMesh = GetWarningFillMesh();
            _warningFill = fillObject.AddComponent<MeshRenderer>();
            _warningFill.sharedMaterial = GetWarningMaterial();
            _warningFill.sortingOrder = FillSortOrder;
            _warningFill.enabled = false;
            return _warningFill;
        }

        private Material GetWarningMaterial()
        {
            if (warningMaterial != null)
                return warningMaterial;

            if (_runtimeWarningMaterial != null)
                return _runtimeWarningMaterial;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                return null;

            _runtimeWarningMaterial = new Material(shader);
            return _runtimeWarningMaterial;
        }

        private Mesh GetWarningFillMesh()
        {
            if (_warningFillMesh != null)
                return _warningFillMesh;

            Vector3[] vertices = new Vector3[WarningSegments + 1];
            int[] triangles = new int[WarningSegments * 3];
            vertices[0] = Vector3.zero;

            for (int i = 0; i < WarningSegments; i++)
            {
                float angle = i / (float)WarningSegments * Mathf.PI * 2f;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

                int triangleIndex = i * 3;
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = i + 1;
                triangles[triangleIndex + 2] = i == WarningSegments - 1 ? 1 : i + 2;
            }

            _warningFillMesh = new Mesh
            {
                name = "PoisonWarningFillMesh",
                vertices = vertices,
                triangles = triangles
            };
            _warningFillMesh.RecalculateBounds();
            return _warningFillMesh;
        }

        private void ApplyPoisonDamage(Vector2 center)
        {
            int hitCount = Physics2D.OverlapCircleNonAlloc(center, poisonImpactRadius, _poisonHits, poisonTargetLayer);
            if (hitCount <= 0)
                return;

            _damagedTargets.Clear();
            DamageData damage = CombatCalculator != null
                ? CombatCalculator.CalculateDamage(poisonDamage)
                : new DamageData { Damage = poisonDamage };

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = _poisonHits[i];
                if (hit == null)
                    continue;
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    continue;
                if (!hit.TryGetComponent(out EntityHealthModule health))
                    continue;
                if (!_damagedTargets.Add(health))
                    continue;

                health.ApplyDamage(damage, hit.ClosestPoint(center), OwnerEntity);
            }
        }

        public override void StopSkill()
        {
            if (_poisonRainCoroutine != null)
            {
                StopCoroutine(_poisonRainCoroutine);
                _poisonRainCoroutine = null;
            }

            if (_warningLine != null)
                _warningLine.enabled = false;
            if (_warningFill != null)
                _warningFill.enabled = false;
        }

        private void OnDisable()
        {
            StopSkill();
        }

        private void OnDestroy()
        {
            StopSkill();

            if (_runtimeWarningMaterial != null)    
                Destroy(_runtimeWarningMaterial);
            if (_warningLine != null)
                Destroy(_warningLine.gameObject);
            if (_warningFill != null)
                Destroy(_warningFill.gameObject);
            if (_warningFillMesh != null)
                Destroy(_warningFillMesh);
        }
    }
}

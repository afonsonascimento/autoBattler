using NarutoAutoBattle.Board;
using NarutoAutoBattle.Core;
using NarutoAutoBattle.Economy;
using NarutoAutoBattle.Units;
using NarutoAutoBattle.View;
using UnityEngine;

namespace NarutoAutoBattle.Input
{
    public class UnitDragController : MonoBehaviour
    {
        [SerializeField] Camera mainCamera;
        [SerializeField] BoardGrid boardGrid;
        [SerializeField] BenchGrid benchGrid;
        [SerializeField] UnitSellService sellService;
        [SerializeField] LayerMask unitLayer = ~0;

        Unit draggedUnit;
        GridSlot dragOriginSlot;
        bool draggingEnabled = true;
        Plane dragPlane;
        Vector3 dragPivotOffset;
        float dragPlaneHeight;

        public bool DraggingEnabled
        {
            get => draggingEnabled;
            set => draggingEnabled = value;
        }

        public bool IsDragging => draggedUnit != null;

        public bool IsDraggingOverShop =>
            IsDragging && ShopUIRegion.ContainsScreenPoint(UnityEngine.Input.mousePosition);

        void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (sellService == null)
                sellService = FindFirstObjectByType<UnitSellService>();

            dragPlane = new Plane(Vector3.up, Vector3.zero);
        }

        void Update()
        {
            if (!draggingEnabled)
                return;

            if (UnityEngine.Input.GetMouseButtonDown(1))
                TrySellUnderCursor();

            if (UnityEngine.Input.GetMouseButtonDown(0))
                TryBeginDrag();

            if (draggedUnit != null && UnityEngine.Input.GetMouseButton(0))
                UpdateDragPosition();

            if (draggedUnit != null && UnityEngine.Input.GetMouseButtonUp(0))
                EndDrag();
        }

        void TrySellUnderCursor()
        {
            if (sellService == null || !sellService.CanSell)
                return;

            var unit = RaycastUnit();
            if (unit == null || !PlacementValidator.CanPlayerDrag(unit))
                return;

            if (sellService.TrySell(unit))
                UnitMergeService.Instance?.ProcessMerges();
        }

        void TryBeginDrag()
        {
            var unit = RaycastUnit();
            if (!PlacementValidator.CanPlayerDrag(unit))
                return;

            draggedUnit = unit;
            dragOriginSlot = unit.CurrentSlot;
            dragPlaneHeight = unit.transform.position.y;
            dragPlane = new Plane(Vector3.up, new Vector3(0f, dragPlaneHeight, 0f));
            dragPivotOffset = GetDragFollowOffset(unit);

            UpdateDragPosition();
        }

        static Vector3 GetDragFollowOffset(Unit unit)
        {
            var renderers = unit.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return Vector3.zero;

            var hasBounds = false;
            var bounds = new Bounds();

            foreach (var renderer in renderers)
            {
                if (!renderer.enabled || renderer.transform.name == "StarIndicator")
                    continue;

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
                return Vector3.zero;

            var offset = bounds.center - unit.transform.position;
            offset.y = 0f;
            return offset;
        }

        Unit RaycastUnit()
        {
            if (mainCamera == null)
                return null;

            var ray = mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 100f, unitLayer))
                return null;

            return hit.collider.GetComponentInParent<Unit>();
        }

        public Unit GetPlayerUnitUnderCursor(bool prepPhaseOnly = true)
        {
            var unit = RaycastUnit();
            if (unit == null || unit.Owner != UnitOwner.Player)
                return null;

            if (prepPhaseOnly && unit.IsInCombat)
                return null;

            return unit;
        }

        void UpdateDragPosition()
        {
            var ray = mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (!dragPlane.Raycast(ray, out float distance))
                return;

            var point = ray.GetPoint(distance);
            draggedUnit.transform.position = new Vector3(
                point.x - dragPivotOffset.x,
                dragPlaneHeight,
                point.z - dragPivotOffset.z);
        }

        void EndDrag()
        {
            if (TrySellViaShopDrop())
            {
                draggedUnit = null;
                dragOriginSlot = null;
                return;
            }

            var worldPos = draggedUnit.transform.position;
            worldPos.y = dragOriginSlot != null ? dragOriginSlot.WorldPosition.y : dragPlaneHeight;

            var targetSlot = FindNearestValidSlot(worldPos);

            if (targetSlot != null && PlacementValidator.CanDropOnSlot(draggedUnit, targetSlot))
            {
                var container = GetContainerForSlot(targetSlot);
                container.TryPlaceUnit(draggedUnit, targetSlot);
                UnitMergeService.Instance?.ProcessMerges();
            }
            else
            {
                ReturnToOrigin();
            }

            draggedUnit = null;
            dragOriginSlot = null;
        }

        bool TrySellViaShopDrop()
        {
            if (sellService == null || !sellService.CanSell)
                return false;

            if (!ShopUIRegion.ContainsScreenPoint(UnityEngine.Input.mousePosition))
                return false;

            if (sellService.TrySell(draggedUnit))
            {
                UnitMergeService.Instance?.ProcessMerges();
                return true;
            }

            ReturnToOrigin();
            return true;
        }

        void ReturnToOrigin()
        {
            if (dragOriginSlot == null)
                return;

            var container = GetContainerForSlot(dragOriginSlot);
            container.TryPlaceUnit(draggedUnit, dragOriginSlot, swap: false);
        }

        GridSlot FindNearestValidSlot(Vector3 worldPosition)
        {
            GridSlot benchSlot = benchGrid.GetSlotAtWorldPosition(worldPosition);
            if (benchSlot != null)
                return benchSlot;

            return boardGrid.GetSlotAtWorldPosition(worldPosition);
        }

        IGridContainer GetContainerForSlot(GridSlot slot)
        {
            return slot.Zone == GridZone.Bench ? benchGrid : boardGrid;
        }
    }
}

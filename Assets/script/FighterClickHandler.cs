using UnityEngine;
using UnityEngine.EventSystems;

public class FighterClickHandler : MonoBehaviour, IPointerClickHandler
{
    private TurnBasedCombat combatController;
    private FighterStats stats;

    public void Initialize(TurnBasedCombat controller, FighterStats fighterStats)
    {
        combatController = controller;
        stats = fighterStats;
    }

    private void HandleClick()
    {
        if (stats == null || combatController == null)
        {
            Debug.LogWarning($"FighterClickHandler: �� �������� �� {gameObject.name}!");
            return;
        }

        if (stats.isPlayer)
            combatController.SelectPlayerFighter(stats, true); // ������� true ��� ��������� ������
        else
            combatController.SelectEnemyTarget(stats);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HandleClick();
    }

    private void OnMouseDown()
    {
        HandleClick();
    }
}
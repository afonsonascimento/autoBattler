using System.Collections.Generic;
using NarutoAutoBattle.Core;
using NarutoAutoBattle.Economy;
using NarutoAutoBattle.Input;
using NarutoAutoBattle.Synergies;
using NarutoAutoBattle.Units;
using UnityEngine;

namespace NarutoAutoBattle.View
{
    public class GameUI : MonoBehaviour
    {
        const float ShopPanelHeight = 150f;

        [SerializeField] GameManager gameManager;
        [SerializeField] ShopService shopService;
        [SerializeField] PlayerGold playerGold;
        [SerializeField] SynergyService synergyService;
        [SerializeField] UnitDragController dragController;
        [SerializeField] UnitSellService sellService;
        [SerializeField] PlayerEconomy playerEconomy;

        int gold;
        GamePhase phase;
        string combatResultMessage;
        float combatResultTimer;
        string tooltipText;
        string economyLine = string.Empty;
        GUIStyle headerStyle;
        GUIStyle shopButtonStyle;
        GUIStyle synergyActiveStyle;
        GUIStyle synergyInactiveStyle;
        GUIStyle resultStyle;
        GUIStyle sellHighlightStyle;
        GUIStyle tooltipStyle;

        void Awake()
        {
            if (dragController == null)
                dragController = FindFirstObjectByType<UnitDragController>();

            if (sellService == null)
                sellService = FindFirstObjectByType<UnitSellService>();

            if (playerEconomy == null)
                playerEconomy = FindFirstObjectByType<PlayerEconomy>();

            playerGold.OnGoldChanged += HandleGoldChanged;
            shopService.OnShopChanged += Repaint;
            gameManager.OnPhaseChanged += HandlePhaseChanged;

            if (playerEconomy != null)
            {
                playerEconomy.OnStateChanged += RefreshEconomyLine;
                RefreshEconomyLine();
            }
        }

        void Start()
        {
            gold = playerGold.Gold;
            phase = gameManager.CurrentPhase;
        }

        void Update()
        {
            if (combatResultTimer > 0f)
                combatResultTimer -= Time.deltaTime;
        }

        void HandleGoldChanged(int newGold)
        {
            gold = newGold;
            RefreshEconomyLine();
        }

        void OnDestroy()
        {
            if (playerGold != null)
                playerGold.OnGoldChanged -= HandleGoldChanged;
            if (shopService != null)
                shopService.OnShopChanged -= Repaint;
            if (gameManager != null)
                gameManager.OnPhaseChanged -= HandlePhaseChanged;
            if (playerEconomy != null)
                playerEconomy.OnStateChanged -= RefreshEconomyLine;
        }

        void RefreshEconomyLine()
        {
            if (playerEconomy == null)
            {
                economyLine = string.Empty;
                return;
            }

            var streak = playerEconomy.GetStreakText();
            economyLine = $"{playerEconomy.GetLevelProgressText()}  |  +{playerEconomy.PredictedInterest}g interest";
            if (!string.IsNullOrEmpty(streak))
                economyLine += $"  |  {streak}";
        }

        void HandlePhaseChanged(GamePhase newPhase)
        {
            phase = newPhase;

            if (newPhase == GamePhase.Prep && gameManager.RoundManager != null)
            {
                var income = gameManager.RoundManager.LastRoundIncome;
                if (income.TotalGold > 0)
                {
                    string prefix = gameManager.LastCombatWon ? "Victory! " : "Defeat. ";
                    combatResultMessage = prefix + income;
                    combatResultTimer = 2.5f;
                }
            }
        }

        void OnGUI()
        {
            EnsureStyles();

            if (gameManager == null)
                return;

            var round = gameManager.RoundManager != null ? gameManager.RoundManager.RoundNumber : 1;
            var losses = gameManager.RoundManager != null ? gameManager.RoundManager.PlayerLosses : 0;
            GUI.Label(new Rect(10, 10, Screen.width - 20, 28),
                $"Naruto Auto Battle  |  Gold: {gold}  |  Round: {round}  |  Losses: {losses}/5  |  {phase}",
                headerStyle);

            if (!string.IsNullOrEmpty(economyLine))
            {
                GUI.Label(new Rect(10, 32, Screen.width - 20, 22), economyLine, synergyInactiveStyle);
            }

            if (phase == GamePhase.Prep)
            {
                DrawSynergyPanel();
                tooltipText = DrawShopPanel();
                DrawUnitTooltip();
            }
            else
            {
                tooltipText = null;
            }

            if (!string.IsNullOrEmpty(tooltipText))
                DrawTooltip(tooltipText);

            if (combatResultTimer > 0f && !string.IsNullOrEmpty(combatResultMessage))
            {
                var rect = new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.35f, 300f, 50f);
                GUI.Box(rect, combatResultMessage, resultStyle);
            }
        }

        void DrawSynergyPanel()
        {
            if (synergyService == null)
                return;

            var synergies = synergyService.GetPlayerActiveSynergies();
            float panelWidth = 220f;
            float x = Screen.width - panelWidth - 10f;
            float y = 58f;

            GUI.Box(new Rect(x, y, panelWidth, 24f), "Active Clans");
            y += 28f;

            if (synergies.Count == 0)
            {
                GUI.Label(new Rect(x + 8f, y, panelWidth - 16f, 40f),
                    "Place ninjas on the board to activate clan bonuses.",
                    synergyInactiveStyle);
                return;
            }

            foreach (var synergy in synergies)
            {
                string bonusText = FormatBonus(synergy);
                GUI.Label(new Rect(x + 8f, y, panelWidth - 16f, 22f),
                    $"{synergy.Clan.displayName} ({synergy.Count}) — {bonusText}",
                    synergyActiveStyle);
                y += 22f;
            }
        }

        static string FormatBonus(ActiveSynergy synergy)
        {
            var parts = new List<string>();
            if (synergy.AttackBonus > 0f)
                parts.Add($"+{Mathf.RoundToInt(synergy.AttackBonus * 100f)}% ATK");
            if (synergy.HealthBonus > 0f)
                parts.Add($"+{Mathf.RoundToInt(synergy.HealthBonus * 100f)}% HP");
            return parts.Count > 0 ? string.Join(", ", parts) : "Active";
        }

        string DrawShopPanel()
        {
            float y = Screen.height - ShopPanelHeight;
            var panelRect = new Rect(0, y, Screen.width, ShopPanelHeight);
            ShopUIRegion.SetPanelRect(panelRect);

            bool sellHover = dragController != null && dragController.IsDraggingOverShop;
            if (sellHover)
                GUI.Box(panelRect, GUIContent.none, sellHighlightStyle ??= CreateSellHighlightStyle());
            else
                GUI.Box(panelRect, GUIContent.none);

            GUI.Label(new Rect(12, y + 8, 720, 24),
                $"Shop  (Lv.{playerEconomy?.Level ?? 1} odds)  — drag unit here or right-click to sell", headerStyle);

            var offers = shopService.CurrentOffers;
            float buttonWidth = 130f;
            float buttonHeight = 60f;
            float rowY = y + 40f;
            float spacing = 8f;
            string shopTooltip = null;

            if (offers.Count == 0)
            {
                GUI.Label(new Rect(12, rowY, Screen.width - 24, 40),
                    "Shop is empty — run Naruto Auto Battle > Setup MVP Scene.");
            }

            var mousePos = GetGuiMousePosition();

            for (int i = 0; i < offers.Count && i < 5; i++)
            {
                float x = 12 + i * (buttonWidth + spacing);
                var rect = new Rect(x, rowY, buttonWidth, buttonHeight);

                if (offers[i] != null)
                {
                    var offer = offers[i];
                    string clans = offer.clans != null && offer.clans.Length > 0
                        ? offer.clans[0].displayName
                        : "";
                    string jutsu = !string.IsNullOrEmpty(offer.jutsuName) ? $"\n{offer.jutsuName}" : "";
                    string label = string.IsNullOrEmpty(clans)
                        ? $"{offer.displayName}\n{offer.cost}g{jutsu}"
                        : $"{offer.displayName}\n{offer.cost}g · {clans}{jutsu}";
                    if (GUI.Button(rect, label, shopButtonStyle))
                        shopService.TryBuy(i);

                    if (rect.Contains(mousePos))
                        shopTooltip = UnitTooltipFormatter.FromData(offer);
                }
                else
                {
                    GUI.Box(rect, "Empty");
                }
            }

            float actionX = Screen.width - 260f;
            if (GUI.Button(new Rect(actionX, rowY, 110, buttonHeight), $"Reroll\n{shopService.RerollCostAmount}g", shopButtonStyle))
                shopService.TryReroll();

            if (GUI.Button(new Rect(actionX + 120f, rowY, 120, buttonHeight), "Fight!", shopButtonStyle))
                gameManager.StartFight();

            if (sellHover)
            {
                GUI.Label(new Rect(Screen.width * 0.5f - 80f, y + 8, 160, 24),
                    "Release to sell", sellHighlightStyle);
            }

            return shopTooltip;
        }

        void DrawUnitTooltip()
        {
            if (dragController == null || dragController.IsDragging)
                return;

            var unit = dragController.GetPlayerUnitUnderCursor();
            if (unit == null)
                return;

            int sellValue = sellService != null ? sellService.GetSellValue(unit) : unit.Data.cost * unit.StarLevel;
            tooltipText = UnitTooltipFormatter.FromUnit(unit, sellValue);
        }

        void DrawTooltip(string text)
        {
            EnsureStyles();

            var content = new GUIContent(text);
            var size = tooltipStyle.CalcSize(content);
            float width = Mathf.Min(size.x + 16f, 320f);
            float height = size.y + 12f;

            float x = GetGuiMousePosition().x + 14f;
            float y = GetGuiMousePosition().y + 14f;

            if (x + width > Screen.width)
                x = Screen.width - width - 8f;
            if (y + height > Screen.height)
                y = Screen.height - height - 8f;

            var rect = new Rect(x, y, width, height);
            GUI.Box(rect, text, tooltipStyle);
        }

        static Vector2 GetGuiMousePosition()
        {
            var pos = UnityEngine.Input.mousePosition;
            return new Vector2(pos.x, Screen.height - pos.y);
        }

        static GUIStyle CreateSellHighlightStyle()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.textColor = new Color(1f, 0.85f, 0.3f);
            style.normal.background = Texture2D.grayTexture;
            return style;
        }

        void EnsureStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                };
            }

            if (shopButtonStyle == null)
            {
                shopButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 12,
                    wordWrap = true,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (synergyActiveStyle == null)
            {
                synergyActiveStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(0.9f, 1f, 0.5f) }
                };
            }

            if (synergyInactiveStyle == null)
            {
                synergyInactiveStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    wordWrap = true,
                    normal = { textColor = new Color(0.75f, 0.75f, 0.75f) }
                };
            }

            if (resultStyle == null)
            {
                resultStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (tooltipStyle == null)
            {
                tooltipStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 12,
                    wordWrap = true,
                    alignment = TextAnchor.UpperLeft,
                    padding = new RectOffset(8, 8, 6, 6),
                    normal = { textColor = Color.white }
                };
            }
        }

        void Repaint() { }
    }
}

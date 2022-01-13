﻿using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VSEWW
{
    internal class Window_WaveCounter : Window
    {
        public override Vector2 InitialSize => new Vector2(400f, 300f);

        private readonly MapComponent_Winston mcw;

        public Window_WaveCounter(MapComponent_Winston mapComponent_Winston)
        {
            this.mcw = mapComponent_Winston;
            this.forcePause = false;
            this.absorbInputAroundWindow = false;
            this.closeOnCancel = false;
            this.closeOnClickedOutside = false;
            this.doCloseButton = false;
            this.doCloseX = false;
            this.draggable = true;
            this.drawShadow = false;
            this.preventCameraMotion = false;
            this.resizeable = false;
            this.doWindowBackground = false;
            this.layer = WindowLayer.GameUI;
        }

        public override void WindowOnGUI()
        {
            if (WorldRendererUtility.WorldRenderedNow)
                return;
            base.WindowOnGUI();
        }

        public override void PostClose()
        {
            base.PostClose();
            mcw.waveCounter = null;
        }

        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            windowRect.x = (float)(UI.screenWidth - windowRect.width - 5f);
            windowRect.y = 5f;
        }

        public void UpdateHeight()
        {
            windowRect.height = 35f + 142f + mcw.nextRaidInfo.kindListLines * 15f;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (VESWWMod.settings.drawBackground)
            {
                Color c = new Color
                {
                    r = Widgets.WindowBGFillColor.r,
                    g = Widgets.WindowBGFillColor.g,
                    b = Widgets.WindowBGFillColor.b,
                    a = 0.25f
                };
                Widgets.DrawBoxSolid(inRect.ExpandedBy(5), c);
            }

            if (mcw.nextRaidInfo.sent)
                DoWaveProgressUI(inRect);
            else
                DoWavePredictionUI(inRect);
        }

        private float DoWaveNumberAndModifierUI(Rect rect)
        {
            var prevFont = Text.Font;
            var prevAnch = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;

            // Modifiers and wave rect
            float mWidth = rect.height - 10;
            int i;
            for (i = 1; i <= mcw.nextRaidInfo.ModifierCount; i++)
            {
                Rect mRect = new Rect(rect)
                {
                    x = rect.xMax - (i * mWidth) - ((i - 1) * 5),
                    width = mWidth,
                    height = mWidth,
                };
                mRect.y += 5;
                GUI.DrawTexture(mRect, Textures.ModifierBGTex);
                mcw.nextRaidInfo.modifiers[i - 1].DrawCard(mRect);
            }

            Rect wRect = new Rect(rect)
            {
                x = rect.xMax - (i * mWidth) - ((i - 1) * 5) - 10,
                width = mWidth + 10,
            };
            GUI.DrawTexture(wRect, Textures.WaveBGTex);
            Widgets.DrawTextureFitted(wRect, mcw.nextRaidInfo.WaveType == 0 ? Textures.NormalTex : Textures.BossTex, 0.8f);

            // Wave number
            Rect waveNumRect = new Rect(rect)
            {
                width = 150f,
            };
            waveNumRect.x = wRect.x - 10 - waveNumRect.width;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(waveNumRect.Rounded(), "VESWW.WaveNum".Translate(mcw.nextRaidInfo.waveNum));

            Text.Font = prevFont;
            Text.Anchor = prevAnch;

            return waveNumRect.x;
        }

        private void DoWavePredictionUI(Rect rect)
        {
            // Wave and modifier
            Rect numRect = new Rect(rect)
            {
                height = 60
            };
            DoWaveNumberAndModifierUI(numRect);
            // Progress bar
            var prevFont = Text.Font;
            var prevAnch = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperRight;

            Rect timeRect = new Rect(rect)
            {
                y = numRect.yMax + 10,
                height = 25
            };
            Widgets.Label(timeRect, mcw.nextRaidInfo.TimeBeforeWave());
            // Kinds
            Text.Font = GameFont.Tiny;
            Rect kindRect = new Rect(rect)
            {
                y = timeRect.yMax + 10,
                height = mcw.nextRaidInfo.kindListLines * 15f//rect.height - numRect.height - timeRect.height - 20,
            };
            Widgets.Label(kindRect, mcw.nextRaidInfo.kindList);
            // Skip wave button
            Rect skipRect = new Rect(rect)
            {
                y = kindRect.yMax + 10,
                x = rect.x + ((rect.width / 3) * 2),
                width = rect.width / 3,
                height = 20f
            };
            if (Widgets.ButtonText(skipRect, "VESWW.SkipWave".Translate()))
            {
                mcw.ExecuteRaid(Find.TickManager.TicksGame);
            }
            // Restore anchor and font size
            Text.Font = prevFont;
            Text.Anchor = prevAnch;
        }

        private void DoWaveProgressUI(Rect rect)
        {
            // Wave and modifier
            Rect numRect = new Rect(rect)
            {
                height = 60
            };
            float startAt = DoWaveNumberAndModifierUI(numRect);
            // Progress bar
            Rect barRect = new Rect(rect)
            {
                x = startAt,
                y = numRect.yMax + 10,
                width = rect.width - startAt,
                height = 25
            };

            if (mcw.nextRaidInfo.Lord != null)
            {
                int pKill = mcw.nextRaidInfo.totalPawn - mcw.nextRaidInfo.WavePawnsLeft();
                DrawFillableBar(barRect, $"{pKill}/{mcw.nextRaidInfo.totalPawn}", (float)pKill / mcw.nextRaidInfo.totalPawn);
                // Pawn left
                var prevAnch = Text.Anchor;
                var prevFont = Text.Font;
                Text.Anchor = TextAnchor.UpperRight;
                Text.Font = GameFont.Tiny;
                Rect kindRect = new Rect(rect)
                {
                    y = barRect.yMax + 10,
                    height = rect.height - numRect.height - barRect.height - 20,
                };
                // - Showing label
                Widgets.Label(kindRect, mcw.nextRaidInfo.cacheKindList);
                Text.Font = prevFont;
                Text.Anchor = prevAnch;
            }
        }
    
        private void DrawFillableBar(Rect rect, string label, float percent, bool doBorder = true)
        {
            if (doBorder)
            {
                GUI.DrawTexture(rect, BaseContent.BlackTex);
                rect = rect.ContractedBy(3f);
            }
            GUI.color = Widgets.WindowBGFillColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);

            Rect fillRect = new Rect(rect);
            fillRect.width *= percent;
            GUI.color = new Color(0.48f, 0.24f, 0.24f);
            GUI.DrawTexture(fillRect, BaseContent.WhiteTex);
            GUI.color = Color.white;

            var prevAnch = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, label);
            Text.Anchor = prevAnch;
        }
    }
}

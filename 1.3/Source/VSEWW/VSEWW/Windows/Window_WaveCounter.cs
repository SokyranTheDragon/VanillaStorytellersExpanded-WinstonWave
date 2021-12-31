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
        public override Vector2 InitialSize => new Vector2(500f, 300f);

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
            windowRect.x = (float)(UI.screenWidth - this.InitialSize.x - 2f);
            windowRect.y = 2f;
        }

        public void UpdateHeightAndWidth()
        {
            Log.Message($"{mcw.nextRaidInfo.kindListLines}");
            windowRect.height = 132f + mcw.nextRaidInfo.kindListLines * 18f;

            if (VESWWMod.settings.compactCounter)
                windowRect.width = 300f;
            else
                windowRect.width = InitialSize.x;

            if (VESWWMod.settings.drawCounterBackground)
                windowRect.height += 20f;

            windowRect.x = (float)(UI.screenWidth - windowRect.width - 2f);
            windowRect.y = 2f;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (VESWWMod.settings.drawCounterBackground)
            {
                Color c = Widgets.WindowBGFillColor;
                c.a = 0.25f;
                Widgets.DrawBoxSolid(inRect, c);
                inRect = inRect.ContractedBy(10);
            }

            if (mcw.nextRaidInfo.sent)
                DoWaveProgressUI(inRect);
            else
                DoWavePredictionUI(inRect);
        }

        private void DoWaveNumberAndModifierUI(Rect rect)
        {
            var prevFont = Text.Font;
            var prevAnch = Text.Anchor;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;

            // Wave number
            Rect waveNumRect = new Rect(rect)
            {
                width = rect.width / 3,
            };
            Widgets.Label(waveNumRect.Rounded(), "VESWW.WaveNum".Translate(mcw.nextRaidInfo.waveNum));

            // Modifiers rect
            Rect modifierRect = new Rect(rect)
            {
                width = (rect.width / 3) * 2,
                x = waveNumRect.xMax
            };
            float mWidth = Math.Min(modifierRect.width / 3, modifierRect.height);
            for (int i = 1; i <= 3; i++)
            {
                Rect mRect = new Rect(modifierRect)
                {
                    x = modifierRect.xMax - (i * mWidth) - ((i - 1) * 5),
                    width = mWidth
                };
                Widgets.DrawWindowBackground(mRect);
            }

            Text.Font = prevFont;
            Text.Anchor = prevAnch;
        }

        private void DoWavePredictionUI(Rect rect)
        {
            // Wave and modifier
            Rect numRect = new Rect(rect)
            {
                height = 50
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
            Text.Font = prevFont;
            // Kinds
            Rect kindRect = new Rect(rect)
            {
                y = timeRect.yMax + 10,
                height = rect.height - numRect.height - timeRect.height - 20,
            };
            Widgets.Label(kindRect, mcw.nextRaidInfo.kindList);
            Text.Anchor = prevAnch;
        }

        private void DoWaveProgressUI(Rect rect)
        {
            // Wave and modifier
            Rect numRect = new Rect(rect)
            {
                height = 50
            };
            DoWaveNumberAndModifierUI(numRect);
            // Progress bar
            Rect barRect = new Rect(rect)
            {
                y = numRect.yMax + 10,
                height = 25
            };

            int pKill = mcw.nextRaidInfo.totalPawn - mcw.nextRaidInfo.WavePawnsLeft();
            DrawFillableBar(barRect, $"{pKill}/{mcw.nextRaidInfo.totalPawn}", (float)pKill / mcw.nextRaidInfo.totalPawn);
            // Pawn left
            var prevAnch = Text.Anchor;
            Text.Anchor = TextAnchor.UpperRight;
            Rect kindRect = new Rect(rect)
            {
                y = barRect.yMax + 10,
                height = rect.height - numRect.height - barRect.height - 20,
            };
            // - Showing label
            Widgets.Label(kindRect, mcw.nextRaidInfo.cacheKindList);
            Text.Anchor = prevAnch;
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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TenhouViewer.Mahjong
{
    public enum StepType
    {
        STEP_DRAWTILE,     // Draw fram wall
        STEP_DISCARDTILE,  // Discard
        STEP_DRAWDEADTILE, // Draw from dead wall

        STEP_NAKI,         // Naki

        STEP_RIICHI,       // Riichi!
        STEP_RIICHI1000,   // Pay for riichi
        
        STEP_TSUMO,        // Tsumo
        STEP_RON,          // Ron
        STEP_DRAW,         // Draw

        STEP_NEWDORA,      // Open next dora indicator

        STEP_DISCONNECT,   // Player disconnected
        STEP_CONNECT       // Player connected
    }

    class Step
    {
        private int Player = -1;
        private int FromWho = -1;
        private int Tile = -1;
        private int Reason = -1;
        private StepType Type;
        private Naki NakiData = null;

        public Step(int Player)
        {
            this.Player = Player;
        }

        public void DrawTile(int Tile)
        {
            this.Tile = Tile;
            this.Type = StepType.STEP_DRAWTILE;

            Console.WriteLine(String.Format("[{0:d}] Draw {1:s}", Player, new Tile(Tile).TileName));
        }

        public void DrawDeadTile(int Tile)
        {
            this.Tile = Tile;
            this.Type = StepType.STEP_DRAWDEADTILE;

            Console.WriteLine(String.Format("[{0:d}] Draw tile from dead wall {1:s}", Player, new Tile(Tile).TileName));
        }

        public void DiscardTile(int Tile)
        {
            this.Tile = Tile;
            this.Type = StepType.STEP_DISCARDTILE;

            Console.WriteLine(String.Format("[{0:d}] Discard {1:s}", Player, new Tile(Tile).TileName));
        }

        public void Naki(Naki Naki)
        {
            this.NakiData = Naki;
            this.Type = StepType.STEP_NAKI;

            Console.WriteLine(String.Format("[{0:d}] Naki {1:s}", Player, Naki.GetText()));
        }

        public void DeclareRiichi()
        {
            this.Type = StepType.STEP_RIICHI;

            Console.WriteLine(String.Format("[{0:d}] Riichi: declare", Player));
        }

        public void PayRiichi()
        {
            this.Type = StepType.STEP_RIICHI1000;

            Console.WriteLine(String.Format("[{0:d}] Riichi: pay 1000", Player));
        }

        public void Ron(int From)
        {
            this.FromWho = From;
            this.Type = StepType.STEP_RON;

            Console.WriteLine(String.Format("[{0:d}] Ron on [{1:d}]", Player, From));
        }

        public void Tsumo()
        {
            this.Type = StepType.STEP_TSUMO;

            Console.WriteLine(String.Format("[{0:d}] Tsumo", Player));
        }

        public void Draw(int Reason)
        {
            this.Reason = Reason;
            this.Type = StepType.STEP_DRAW;

            Console.WriteLine("Draw");
        }

        public void NewDora(int Tile)
        {
            this.Tile = Tile;
            this.Type = StepType.STEP_NEWDORA;

            Console.WriteLine(String.Format("Dora {0:s}", new Tile(Tile).TileName));
        }

        public void Disconnect()
        {
            this.Type = StepType.STEP_DISCONNECT;

            Console.WriteLine(String.Format("[{0:d}] Disconnect", Player));
        }

        public void Connect()
        {
            this.Type = StepType.STEP_CONNECT;

            Console.WriteLine(String.Format("[{0:d}] Connect", Player));
        }
    }
}
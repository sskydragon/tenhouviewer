﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TenhouViewer.Mahjong
{
    class Round
    {
        public string Hash = "";
        public int Index = 0;

        public Wall Wall = new Wall();
        public List<Step> Steps = new List<Step>();
        public Hand[] Hands = new Hand[4]; // Start hands

        public List<Yaku>[] Yaku = new List<Yaku>[4];

        public int[] Pay = new int[4];
        public int[] BalanceBefore = new int[4];
        public int[] BalanceAfter = new int[4];

        public int[] HanCount = new int[4];
        public int[] FuCount = new int[4];
        public int[] Cost = new int[4];

        public int RenchanStick = 0;
        public int RiichiStick = 0;

        public Round()
        {
            for (int i = 0; i < 4; i++)
            {
                Yaku[i] = new List<Yaku>();

                Pay[i] = 0;
                HanCount[i] = 0;
                FuCount[i] = 0;
                Cost[i] = 0;

                BalanceBefore[i] = 0;
                BalanceAfter[i] = 0;
            }
        }

        public void Save(string FileName)
        {
            Xml X = new Xml(FileName);

            X.StartXML("mjround");

            // Что это за раздача?
            X.WriteTag("hash", "value", Hash);
            X.WriteTag("game", "index", Index);

            X.WriteTag("balancebefore", BalanceBefore);
            X.WriteTag("balanceafter", BalanceAfter);
            X.WriteTag("pay", Pay);

            X.WriteTag("riichistick", "value", RenchanStick);
            X.WriteTag("renchanstick", "value", RiichiStick);

            // Actions
            {
                X.StartTag("steps");
                X.Attribute("count", Steps.Count);

                for (int j = 0; j < Steps.Count; j++)
                {
                    Steps[j].WriteXml(X);
                }

                X.EndTag();
            }

            // Yaku list
            {

                for (int i = 0; i < 4; i++)
                {
                    if (Yaku[i].Count == 0) continue;

                    X.StartTag("agari");
                    X.Attribute("player", i);
                    X.Attribute("han", HanCount[i]);
                    X.Attribute("fu", FuCount[i]);
                    X.Attribute("cost", Cost[i]);
                    {
                        X.StartTag("yakulist");
                        X.Attribute("count", Yaku[i].Count);
                        for (int j = 0; j < Yaku[i].Count; j++)
                        {
                            X.StartTag("yaku");

                            X.Attribute("index", Yaku[i][j].Index);
                            X.Attribute("cost", Yaku[i][j].Cost);

                            X.EndTag();
                        }
                        X.EndTag();
                    }
                    X.EndTag();
                }
            }

            X.EndXML();
            X.Close();
        }
    }
}

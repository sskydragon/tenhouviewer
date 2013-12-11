﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TenhouViewer.Search
{
    class GameFinder
    {
        private List<Result> GameList = new List<Result>();

        // Find queries
        // Fields use if they has not default values

        // Hash
        public string Hash = null;

        // NickName
        public string NickName = null;

        // Yaku (hand contain all yaku from this array)
        public int[] YakuList = null;

        // Yaku (hand contain any yaku from this array)
        public int[] AnyYakuList = null;

        // Waitings (hand has at least one tile in waiting from this array)
        public int[] Waitings = null;

        // Player count
        public int PlayerCount = -1;

        // Player place
        public int Place = -1;

        // Rank (1 = 1ku, 10 = 1dan, 11 = 2dan, ...)
        public int Rank = -1;

        // Is player dealer?
        public int Dealer = -1;

        // Is player winner?
        public int Winner = -1;

        // Is player loser (discard to ron)
        public int Loser = -1;

        // Is player furiten
        public int Furiten = -1;

        // Is player declared riichi
        public int Riichi = -1;

        // Is ron-agari
        public int Ron = -1;

        // Is tsumo-agari
        public int Tsumo = -1;

        // Is round end in draw
        public bool Draw = false;
        public int DrawReason = -1;

        // Maximum steps (tile discard) in round
        public int StepsMax = -1;

        // Amount of tiles to wait
        public int WaitingCountMin = -1;
        public int WaitingCountMax = -1;

        // Payment
        public int PaymentMin = -1;
        public int PaymentMax = -1;

        // Player rating
        public int RatingMin = -1;
        public int RatingMax = -1;

        // Shanten of start hand
        public int ShantenMax = -1;
        public int ShantenMin = -1;

        // Han count of winned hand
        public int HanMax = -1;
        public int HanMin = -1;

        // Fu count of winned hand
        public int FuMax = -1;
        public int FuMin = -1;

        // Danger tiles count
        public int DangerMax = -1;
        public int DangerMin = -1;

        // Lobby
        public int Lobby = -1;

        // Lobby type (decoded)
        public int Aka = -1; // 赤, aka-dora
        public int Kuitan = -1; // 喰, open tanyao
        public int Nan = -1; // 東南, hanchan or tonpuusen
        public int Toku = -1; // 特上, expert class lobby type
        public int Saku = -1; // 速, Speedy game
        public int High = -1; // 上級, Advanced class lobby type

        // Winds
        public int PlayerWind = -1;
        public int RoundWind = -1;

        // Riichi count
        public int RiichiCount = -1;

        // Renchan stick count
        public int RenchanStick = -1;

        // Round index (w-1, e-4)
        public int RoundIndex = -1;

        // Naki count
        public int NakiCount = -1;
        public int OpenedSets = -1;

        // Target form
        // -1 - any; 0-4 - mask tile;
        public int[] Form = null;

        public GameFinder(Tenhou.TenhouHashList Hashes)
        {
            // Create blank ResultList from hash table
            foreach (string Hash in Hashes.Hashes)
            {
                Mahjong.Replay R = new Mahjong.Replay();

                if (R.LoadXml(Hash))
                {
                    Result Res = new Result();
                    Res.Replay = R;
                    Res.ReplayMark = true;

                    for (int i = 0; i < 4; i++) Res.PlayerMark[i] = true;
                    // Exclude 4th player in 3man game
                    if (R.PlayerCount == 3) Res.PlayerMark[3] = false;

                    for (int i = 0; i < R.Rounds.Count; i++)
                    {
                        bool[] Marks = new bool[4];
                        for (int j = 0; j < Res.Replay.PlayerCount; j++) Marks[j] = true;

                        Res.RoundMark.Add(true);
                        Res.HandMark.Add(Marks);
                    }

                    GameList.Add(Res);
                }
                else
                {
                    Console.WriteLine(Hash + " - not found");
                }
            }
        }

        // Finder can use results of previous queries
        public GameFinder(List<Result> Results)
        {
            // Reset marks 
            foreach (Result R in Results)
            {
                R.ReplayMark = true;

                for (int i = 0; i < R.RoundMark.Count; i++)
                {
                    //R.RoundMark[i] = true;
                    for (int j = 0; j < R.Replay.PlayerCount; j++) R.HandMark[i][j] = true;
                }

                for (int i = 0; i < R.Replay.PlayerCount; i++) R.PlayerMark[i] = true;

                GameList.Add(R);
            }
        }

        public List<Result> Find()
        {
            List<Result> ResultList = new List<Result>();

            for (int i = 0; i < GameList.Count; i++)
            {
                Result R = GameList[i];

                Console.Title = String.Format("Completed {0:d}/{1:d}, found {2:d}", i, GameList.Count, ResultList.Count);

                // filters
                CheckHash(R);
                CheckPlayerCount(R);
                CheckNickName(R);
                CheckYaku(R);
                CheckHan(R);
                CheckFu(R);
                CheckPlace(R);
                CheckRank(R);
                CheckPayment(R);
                CheckDealer(R);
                CheckWinner(R);
                CheckLoser(R);
                CheckRating(R);
                CheckShanten(R);
                CheckSteps(R);
                CheckWaitings(R);
                CheckDraw(R);
                CheckLobby(R);
                CheckWind(R);
                CheckDangerSteps(R);
                CheckFuriten(R);
                CheckNaki(R);
                CheckRiichi(R);
                CheckAgari(R);
                CheckRound(R);
                CheckForm(R);

                // Check mark
                EmbedMarksToHandMark(R);
                if (!IsQueryOk(R)) continue;

                ResultList.Add(R);
            }

            return ResultList;
        }

        private bool IsQueryOk(Result R)
        {
            if (!R.ReplayMark) return false;
            if (!R.PlayerMark.Contains(true)) return false;

            for (int i = 0; i < R.RoundMark.Count; i++)
            {
                // If any round contain positive result...
                if (R.RoundMark[i]) return true;
            }

            return false;
        }

        private void EmbedMarksToHandMark(Result R)
        {
            for (int i = 0; i < R.RoundMark.Count; i++)
            {
                for (int j = 0; j < R.Replay.PlayerCount; j++)
                {
                    if (!R.PlayerMark[j]) R.HandMark[i][j] = false;
                    if (!R.RoundMark[i]) R.HandMark[i][j] = false;
                }

                // Exclude rounds which has no suitable hands
                R.RoundMark[i] = (R.RoundMark[i] && R.HandMark[i].Contains(true));
            }
        }

        private bool CheckFormInHand(Mahjong.Hand H)
        {
            int[] Tehai = new int[38];

            for (int i = 0; i < Tehai.Length; i++) Tehai[i] = 0;

            // Create tehai
            for (int i = 0; i < H.Tiles.Length; i++)
            {
                if (H.Tiles[i] == -1) continue;

                Mahjong.Tile T = new Mahjong.Tile(H.Tiles[i]);

                Tehai[T.TileId]++;
            }

            for (int i = 0; i < 3; i++)
            {
                bool Mark = true;
                for (int j = 1; j < 10; j++)
                {
                    if (Form[j] == -1) continue;

                    if (Form[j] != Tehai[i * 10 + j])
                    {
                        Mark = false;
                        break;
                    }
                }

                if (Mark) return true;
            }

            return false;
        }

        // filters
        private void CheckForm(Result R)
        {
            if (Form == null) return;

            for (int i = 0; i < R.Replay.Rounds.Count; i++)
            {
                Mahjong.Round Rnd = R.Replay.Rounds[i];

                Rnd.ReplayGame();

                for (int j = 0; j < R.Replay.PlayerCount; j++)
                {
                    bool Mark = false;
                    for (int k = 0; k < Rnd.Hands[j].Count; k++)
                    {
                        if (CheckFormInHand(Rnd.Hands[j][k]))
                        {
                            Mark = true;
                            break;
                        }
                    }

                    if(!Mark) R.HandMark[i][j] = false;
                }
            }
        }

        private void CheckHash(Result R)
        {
            if (Hash == null) return;

            if (R.Replay.Hash.CompareTo(Hash) != 0) R.ReplayMark = false;
        }

        private void CheckPlayerCount(Result R)
        {
            if (PlayerCount == -1) return;

            for (int i = 0; i < R.Replay.Rounds.Count; i++)
            {
                Mahjong.Round Rnd = R.Replay.Rounds[i];

                if (Rnd.PlayerCount != PlayerCount) R.RoundMark[i] = false;
            }
        }

        private void CheckNickName(Result R)
        {
            if (NickName == null) return;

            for (int i = 0; i < R.Replay.PlayerCount; i++)
            {
                if (R.Replay.Players[i].NickName.CompareTo(NickName) != 0)
                    R.PlayerMark[i] = false;
            }
        }

        private void CheckPlace(Result R)
        {
            if (Place == -1) return;

            for (int i = 0; i < R.Replay.PlayerCount; i++)
            {
                if (R.Replay.Place[i] == Place)
                    R.PlayerMark[i] = false;
            }
        }

        private void CheckRound(Result R)
        {
            if (RenchanStick != -1)
            {
                for (int i = 0; i < R.Replay.Rounds.Count; i++)
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[i];

                    if (Rnd.RenchanStick != RenchanStick) R.RoundMark[i] = false;
                }
            }
            if (RoundIndex != -1)
            {
                for (int i = 0; i < R.Replay.Rounds.Count; i++)
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[i];

                    if (Rnd.Index != RoundIndex) R.RoundMark[i] = false;
                }
            }
        }

        private void CheckHan(Result R)
        {
            for (int i = 0; i < R.Replay.Rounds.Count; i++)
            {
                Mahjong.Round Rnd = R.Replay.Rounds[i];

                for (int j = 0; j < R.Replay.PlayerCount; j++)
                {
                    if (HanMin != -1) if (Rnd.HanCount[j] < HanMin) R.HandMark[i][j] = false;
                    if (HanMax != -1) if (Rnd.HanCount[j] > HanMax) R.HandMark[i][j] = false;
                }
            }
        }

        private void CheckFu(Result R)
        {
            for (int i = 0; i < R.Replay.Rounds.Count; i++)
            {
                Mahjong.Round Rnd = R.Replay.Rounds[i];

                for (int j = 0; j < R.Replay.PlayerCount; j++)
                {
                    if (FuMin != -1) if (Rnd.FuCount[j] < FuMin) R.HandMark[i][j] = false;
                    if (FuMax != -1) if (Rnd.FuCount[j] > FuMax) R.HandMark[i][j] = false;
                }
            }
        }

        private void CheckRank(Result R)
        {
            if (Rank == -1) return;

            for (int i = 0; i < R.Replay.PlayerCount; i++)
            {
                if (R.Replay.Players[i].Rank != Rank)
                    R.PlayerMark[i] = false;
            }
        }

        private void CheckLobby(Result R)
        {
            if (Lobby != -1) { if (R.Replay.Lobby != Lobby) R.ReplayMark = false; }

            if(Aka != -1)
            {
                // Check aka-dora setting
                if (!((Aka == 0) && ((R.Replay.LobbyType & 0x0002) == 0x0002))) R.ReplayMark = false;
            }

            if (Kuitan != -1)
            {
                // Check open tanyao setting
                if (!((Kuitan == 0) && ((R.Replay.LobbyType & 0x0004) == 0x0004))) R.ReplayMark = false;
            }

            if (Nan != -1)
            {
                // Check game length setting (
                if (!((Nan != 0) && ((R.Replay.LobbyType & 0x0008) == 0x0008))) R.ReplayMark = false;
            }

            if (Toku != -1)
            {
                // Check lobby level
                if (!((Toku != 0) && ((R.Replay.LobbyType & 0x0020) == 0x0020))) R.ReplayMark = false;
            }

            if (Saku != -1)
            {
                // Check game speed
                if (!((Saku != 0) && ((R.Replay.LobbyType & 0x0040) == 0x0040))) R.ReplayMark = false;
            }

            if (High != -1)
            {
                // Check lobby level
                if (!((High != 0) && ((R.Replay.LobbyType & 0x0080) == 0x0080))) R.ReplayMark = false;
            }
        }

        private void CheckRating(Result R)
        {
            for (int i = 0; i < R.Replay.PlayerCount; i++)
            {
                if (RatingMin != -1)
                {
                    if (R.Replay.Players[i].Rating < RatingMin) R.PlayerMark[i] = false;
                }
                if (RatingMax != -1)
                {
                    if (R.Replay.Players[i].Rating > RatingMax) R.PlayerMark[i] = false;
                }
            }
        }

        private void CheckShanten(Result R)
        {
            for (int i = 0; i < R.Replay.Rounds.Count; i++)
            {
                Mahjong.Round Rnd = R.Replay.Rounds[i];

                for (int j = 0; j < R.Replay.PlayerCount; j++)
                {
                    if (Rnd.Shanten[j].Count > 0)
                    {
                        if (ShantenMin != -1) if (Rnd.Shanten[j][0] < ShantenMin) R.HandMark[i][j] = false;
                        if (ShantenMax != -1) if (Rnd.Shanten[j][0] > ShantenMax) R.HandMark[i][j] = false;
                    }
                    else
                    {
                        R.HandMark[i][j] = false;
                    }
                }
            }
        }

        private void CheckPayment(Result R)
        {
            for (int i = 0; i < R.Replay.Rounds.Count; i++)
            {
                Mahjong.Round Rnd = R.Replay.Rounds[i];

                for (int j = 0; j < R.Replay.PlayerCount; j++)
                {
                    if (PaymentMin != -1)
                    {
                        if (Rnd.Pay[j] < PaymentMin) R.HandMark[i][j] = false;
                    }
                    if (PaymentMax != -1)
                    {
                        if (Rnd.Pay[j] > PaymentMax) R.HandMark[i][j] = false;
                    }
                }
            }
        }

        private void CheckWind(Result R)
        {
            if (RoundWind != -1)
            {
                for (int i = 0; i < R.Replay.Rounds.Count; i++)
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[i];

                    if((Rnd.CurrentRound & 3) != RoundWind) R.RoundMark[i] = false;
                }
            }

            if (PlayerWind != -1)
            {
                for (int i = 0; i < R.Replay.Rounds.Count; i++)
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[i];

                    for (int j = 0; j < R.Replay.PlayerCount; j++)
                    {
                        if (Rnd.Wind[j] != PlayerWind) R.PlayerMark[i] = false;
                    }
                }
            }
        }

        private void CheckAgari(Result R)
        {
            if (Ron != -1)
            {
                bool IsRon = (Ron != 0);

                for (int i = 0; i < R.Replay.Rounds.Count; i++)
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[i];

                    bool RonAgari = (Rnd.Loser.Contains(true));
                    bool Winner = (Rnd.Winner.Contains(true));

                    if ((!Winner) && (RonAgari != IsRon)) R.RoundMark[i] = false;
                }
            }

            if (Tsumo != -1)
            {
                bool IsTsumo = (Tsumo != 0);

                for (int i = 0; i < R.Replay.Rounds.Count; i++)
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[i];

                    bool RonAgari = (Rnd.Loser.Contains(true));
                    bool Winner = (Rnd.Winner.Contains(true));

                    if ((!Winner) && (RonAgari == IsTsumo)) R.RoundMark[i] = false;
                }
            }
        }

        private void CheckWinner(Result R)
        {
            if (Winner == -1) return;

            bool IsWinner = (Winner != 0);
            for (int i = 0; i < R.Replay.Rounds.Count; i++)
            {
                Mahjong.Round Rnd = R.Replay.Rounds[i];

                for (int j = 0; j < R.Replay.PlayerCount; j++)
                {
                    if (Rnd.Winner[j] != IsWinner) R.HandMark[i][j] = false;
                }
            }
        }

        private void CheckDealer(Result R)
        {
            if (Dealer == -1) return;

            bool IsDealer = (Dealer != 0);
            for (int i = 0; i < R.Replay.Rounds.Count; i++)
            {
                Mahjong.Round Rnd = R.Replay.Rounds[i];

                for (int j = 0; j < R.Replay.PlayerCount; j++)
                {
                    if (Rnd.Dealer[j] != IsDealer) R.HandMark[i][j] = false;
                }
            }
        }

        private void CheckLoser(Result R)
        {
            if (Loser == -1) return;
            bool IsLoser = (Loser != 0);

            for (int i = 0; i < R.Replay.Rounds.Count; i++)
            {
                Mahjong.Round Rnd = R.Replay.Rounds[i];

                for (int j = 0; j < R.Replay.PlayerCount; j++)
                {
                    if (Rnd.Loser[j] != IsLoser) R.HandMark[i][j] = false;
                }
            }
        }

        private void CheckRiichi(Result R)
        {
            if (RiichiCount != -1)
            {
                for (int i = 0; i < R.Replay.Rounds.Count; i++)
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[i];
                    int Count = 0;

                    for (int j = 0; j < R.Replay.PlayerCount; j++)
                    {
                        if (Rnd.Riichi[j] >= 0) Count++;
                    }

                    if (Count != RiichiCount) R.RoundMark[i] = false;
                }
            }

            if (Riichi != -1)
            {
                bool IsRiichi = (Riichi != 0);

                for (int i = 0; i < R.Replay.Rounds.Count; i++)
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[i];

                    for (int j = 0; j < R.Replay.PlayerCount; j++)
                    {
                        bool PlayerRiichi = Rnd.Riichi[j] > 0;

                        if (IsRiichi != PlayerRiichi) R.HandMark[i][j] = false;
                    }
                }
            }
        }

        private void CheckNaki(Result R)
        {
            if (NakiCount != -1)
            {
                for (int i = 0; i < R.Replay.Rounds.Count; i++)
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[i];

                    for (int j = 0; j < R.Replay.PlayerCount; j++)
                    {
                        if (Rnd.Naki[j] != NakiCount) R.HandMark[i][j] = false;
                    }
                }
            }

            if (OpenedSets != -1)
            {
                for (int i = 0; i < R.Replay.Rounds.Count; i++)
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[i];

                    for (int j = 0; j < R.Replay.PlayerCount; j++)
                    {
                        if (Rnd.OpenedSets[j] != OpenedSets) R.HandMark[i][j] = false;
                    }
                }
            }
        }

        private void CheckSteps(Result R)
        {
            if (StepsMax == -1) return;

            for (int i = 0; i < R.Replay.Rounds.Count; i++)
            {
                Mahjong.Round Rnd = R.Replay.Rounds[i];

                for (int j = 0; j < R.Replay.PlayerCount; j++)
                {
                    if (Rnd.StepCount[j] > StepsMax) R.HandMark[i][j] = false;
                }
            }
        }

        private void CheckFuriten(Result R)
        {
            if(Furiten == -1) return;

            bool FRef = (Furiten == 1);

            for (int i = 0; i < R.Replay.Rounds.Count; i++)
            {
                Mahjong.Round Rnd = R.Replay.Rounds[i];
                bool[] TempFuriten = new bool[R.Replay.PlayerCount];

                for (int j = 0; j < R.Replay.PlayerCount; j++) TempFuriten[j] = false;

                for (int j = 0; j < Rnd.Steps.Count; j++)
                {
                    Mahjong.Step S = Rnd.Steps[j];
                    if (S.Furiten) TempFuriten[S.Player] = true;
                }

                for (int j = 0; j < R.Replay.PlayerCount; j++)
                {
                    if(TempFuriten[j] != FRef) R.HandMark[i][j] = false;
                }
            }
        }

        private void CheckDangerSteps(Result R)
        {
            for (int i = 0; i < R.Replay.Rounds.Count; i++)
            {
                Mahjong.Round Rnd = R.Replay.Rounds[i];
                int[] MaxDangerTiles = new int[R.Replay.PlayerCount];

                for (int j = 0; j < R.Replay.PlayerCount; j++)
                    MaxDangerTiles[j] = 0;

                for (int j = 0; j < Rnd.Steps.Count; j++)
                {
                    int[] D = Rnd.Steps[j].Danger;
                    if (D == null) continue;

                    int Player = Rnd.Steps[j].Player;

                    if (MaxDangerTiles[Player] < D.Length) MaxDangerTiles[Player] = D.Length;
                }
                
                    
                for (int j = 0; j < R.Replay.PlayerCount; j++)
                {
                    if (!(((MaxDangerTiles[j] >= DangerMin) || (DangerMin == -1)) &&
                         ((MaxDangerTiles[j] <= DangerMax) || (DangerMax == -1))))
                        R.HandMark[i][j] = false;
                }
            }
        }

        private void CheckDraw(Result R)
        {
            if (!Draw) return;

            for (int i = 0; i < R.Replay.Rounds.Count; i++)
            {
                Mahjong.Round Rnd = R.Replay.Rounds[i];
                if (Rnd.Result == Mahjong.RoundResult.Draw)
                {
                    if (DrawReason == Rnd.DrawReason)
                    {
                        switch (DrawReason)
                        {
                            case -1: continue; // normal;
                            case 0:            // 9 hai
                                int LastTsumoIndex = -1;
                                for (int j = 0; j < Rnd.Steps.Count; j++)
                                {
                                    if (Rnd.Steps[j].Type == Mahjong.StepType.STEP_DRAWTILE) LastTsumoIndex = j;
                                }

                                if (LastTsumoIndex >= 0)
                                {
                                    int Player = Rnd.Steps[LastTsumoIndex].Player;

                                    for (int j = 0; j < R.Replay.PlayerCount; j++)
                                    {
                                        if (j != Player) R.HandMark[i][j] = false;
                                    }
                                }
                                continue;
                            case 1: continue;  // reach 4
                            case 2: continue;  // ron 3
                            case 3: continue;  // kan 4
                            case 4: continue;  // kaze 4
                            case 5:            // nm
                                for (int j = 0; j < R.Replay.PlayerCount; j++)
                                {
                                    if (Rnd.Pay[j] < 0) R.HandMark[i][j] = false;
                                }
                                continue;
                        }
                    }
                }

                R.RoundMark[i] = false;
            }
        }

        private void CheckWaitings(Result R)
        {
            if (!((WaitingCountMin == -1) && (WaitingCountMax == -1)))
            {

                for (int i = 0; i < R.Replay.Rounds.Count; i++)
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[i];

                    for (int j = 0; j < R.Replay.PlayerCount; j++)
                    {
                        if (Rnd.WinWaiting[j].Count > WaitingCountMax) R.HandMark[i][j] = false;
                        if (Rnd.WinWaiting[j].Count < WaitingCountMin) R.HandMark[i][j] = false;
                    }
                }
            }

            if (Waitings != null)
            {
                for (int i = 0; i < R.Replay.Rounds.Count; i++)
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[i];

                    for (int j = 0; j < R.Replay.PlayerCount; j++)
                    {
                        bool HasWait = false;
                        for (int k = 0; k < Rnd.WinWaiting[j].Count; k++)
                        {
                            if(Waitings.Contains(Rnd.WinWaiting[j][k]))
                            {
                                HasWait = true;
                                break;
                            }
                        }

                        if (!HasWait) R.HandMark[i][j] = false;
                    }
                }
            }
        }

        private bool IsYakuInYakuList(List<Mahjong.Yaku> YakuList, int Yaku)
        {
            for (int i = 0; i < YakuList.Count; i++)
            {
                if(YakuList[i].Index == Yaku) return true;
            }

            return false;
        }

        private void CheckYaku(Result R)
        {
            if (YakuList != null)
            {
                for (int RoundIndex = 0; RoundIndex < R.Replay.Rounds.Count; RoundIndex++ )
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[RoundIndex];
                    for (int i = 0; i < R.Replay.PlayerCount; i++)
                    {
                        for (int j = 0; j < YakuList.Length; j++)
                        {
                            if (!IsYakuInYakuList(Rnd.Yaku[i], YakuList[j]))
                            {
                                R.HandMark[RoundIndex][i] = false;
                                break;
                            }
                        }
                    }
                }
            }

            if (AnyYakuList != null)
            {
                for (int RoundIndex = 0; RoundIndex < R.Replay.Rounds.Count; RoundIndex++)
                {
                    Mahjong.Round Rnd = R.Replay.Rounds[RoundIndex];
                    for (int i = 0; i < R.Replay.PlayerCount; i++)
                    {
                        bool HasYaku = false;

                        for (int j = 0; j < AnyYakuList.Length; j++)
                        {
                            if (IsYakuInYakuList(Rnd.Yaku[i], AnyYakuList[j]))
                            {
                                HasYaku = true;
                                break;
                            }
                        }

                        if (!HasYaku) R.HandMark[RoundIndex][i] = false;
                    }
                }
            }
        }

    }
}

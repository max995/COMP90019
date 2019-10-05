﻿/*
 * The vanilla AI Agent
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIAgent : MonoBehaviour
{
    // trueStart, trueEnd, fakeStart, fakeEnd
    private List<Vector3> anchor = new List<Vector3>();
    //private int trueStart = 0, trueEnd = 1, fakeStart = 2, fakeEnd = 3;
    //private int neighborRange = 3;

    //private float trueDepositDelay = 0.1f;
    //private float fakeDepositDelay = 1f;
    private int FindChainCost(Vector3 start, Vector3 end, bool onlyRed)
    {
        List<Vector3> path = Methods.instance.FindPathInGrid(start, end, onlyRed);
        List<Vector3> emptyPos = Methods.instance.RemoveDepositedAndAnchor(path);
        return emptyPos.Count;
    }

    private Vector3[] FindCheapestChain(Vector3[] except, bool onlyRed)
    {
        Vector3[] anchors = new Vector3[2];
        int cheapestCost = int.MaxValue;
        foreach (Vector3 anchorCenter1 in GameManager.instance.anchorPositions)
        {
            foreach (Vector3 anchorCenter2 in GameManager.instance.anchorPositions)
            {
                if (anchorCenter1 == anchorCenter2) continue;
                Vector3 anchor1 = Methods.instance.TransAnchorPositionInGrid(anchorCenter1);
                Vector3 anchor2 = Methods.instance.TransAnchorPositionInGrid(anchorCenter2);
                //Methods.instance.TilesNarrativeRegion(anchorCenter1,anchorCenter2);
                int cost = FindChainCost(anchor1, anchor2, onlyRed);
                if (cost != 0 && cost < cheapestCost && !(except[0] == anchor1 && except[1] == anchor2) && !(except[0] == anchor2 && except[1] == anchor1))
                {
                    cheapestCost = cost;
                    anchors[0] = anchor1;
                    anchors[1] = anchor2;
                }
            }
        }
        return anchors;
    }

    private List<Vector3> RemovePositionsFromList(List<Vector3> list, List<Vector3> positions)
    {
        List<Vector3> validList = new List<Vector3>();
        for (int i = 0; i < list.Count; i++)
        {
            if (!positions.Contains(list[i]))
            {
                validList.Add(list[i]);
            }
        }
        return validList;
    }

    private Vector3 GetRandomEmptyGrid(List<Actions> AIactions, Actions actions)
    {
        int x = Random.Range(0, GameParameters.instance.gridSize), y = Random.Range(0, GameParameters.instance.gridSize);
        List<Vector3> positions = actions.GetDepositPos(AIactions);
        while (!Methods.instance.IsEmptyGrid(new Vector3(x, y, 0f)) || positions.Contains(new Vector3(x, y, 0f)))
        {
            x = Random.Range(0, GameParameters.instance.gridSize);
            y = Random.Range(0, GameParameters.instance.gridSize);
        }
        return new Vector3(x, y, 0f);
    }

    private List<int> RandomOrder(int num)
    {
        List<int> index = new List<int>();
        for (int i = 0; i < num; i++)
        {
            index.Add(i);
        }
        
        return index;
    }
    private List<int> InOrder(int num)
    {
        List<int> index = new List<int>();
        for (int i = 0; i < num; i++)
        {
            index.Add(i);
        }
        List<int> randomOrder = new List<int>();

        while (index.Count > 0)
        {
            int i = Random.Range(0, index.Count);
            randomOrder.Add(index[i]);
            index.RemoveAt(i);
        }
        return randomOrder;
    }
    private List<Vector3> GetAllRedPickups()
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < GameManager.instance.generators.Count; i++)
        {
            List<GameObject> pickups = GameManager.instance.generators[i].GetComponent<GeneratorManager>().GetPickupsInGn();
            foreach (GameObject pickup in pickups)
            {
                if (GameManager.instance.generators[i].GetComponent<GeneratorManager>().GetPickupsInGnColor(pickup.transform.position) == 0)
                {
                    positions.Add(pickup.transform.position);
                }
            }
        }
        return positions;
    }

    private List<Vector3> GetUselessRedCounters(Vector3[] anchors)
    {
        List<Vector3> uselessRedCounters = new List<Vector3>();
        List<Vector3> usedRedCounters = Methods.instance.FindPathInGrid(anchors[0], anchors[1], true);
        for (int x = 0; x < GameParameters.instance.gridSize; x++)
        {
            for (int y = 0; y < GameParameters.instance.gridSize; y++)
            {
                if (GameManager.instance.deposited[x][y] == 0 && !usedRedCounters.Contains(new Vector3(x, y, 0f)))
                {
                    uselessRedCounters.Add(new Vector3(x, y, 0f));
                }
            }
        }
        return uselessRedCounters;
    }

    private void Start()
    {
        Methods.instance.Task1Anchor(GameManager.instance.anchorPositions, out GameManager.instance.Task1_a, out GameManager.instance.Task1_b);

    }

    public Actions MakeDecision(List<Actions> AIactions, int turn)

    {
        Actions actions = new Actions();
        Debug.Log("now is turn :" + turn);
       
        //Vector3[] trueAnchors;
        Vector3[] closestAnchorsRed = FindCheapestChain(new Vector3[2], true);

        // Check if can win the game this turn
        //List<Vector3> uselessRedCounters = GetUselessRedCounters(closestAnchorsRed);
        List<Vector3> redPickups = GetAllRedPickups();
        int minCost1 = FindChainCost(GameManager.instance.anchor_a1, GameManager.instance.anchor_a2, true);
        
        //Methods.instance.TilesNarrativeRegion(GameManager.instance.anchor_a1, GameManager.instance.anchor_a2);
        int minCost2 = FindChainCost(GameManager.instance.anchor_a3, GameManager.instance.anchor_a4, true);
        List<Vector3> pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(GameManager.instance.anchor_a1, GameManager.instance.anchor_a2, true));
        List<Vector3> pathB = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(GameManager.instance.anchor_a3, GameManager.instance.anchor_a4, true));
        Debug.Log("minCost1: " + minCost1 + "  " + "minCost2 :" + minCost2 + "  ");
        //"useless: " + uselessRedCounters.Count);
        if (minCost1 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost1 && GameManager.instance.pathChange == 0)
        //+uselessRedCounters.Count >= minCost)
        {
            //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
            // Collect all red pickups from generators
            int i = 0;
            while (i < redPickups.Count)
            {
                if (!actions.GetCollectPos(AIactions).Contains(redPickups[i]) && actions.GetPickupColor().Sum() < GameParameters.instance.carryLimit && actions.GetCollectPos(AIactions).Count < minCost1)
                {
                    actions.MoveTo(GameManager.instance.parkingPos[Methods.instance.OnPickup(redPickups[i])]);
                    actions.CollectAt(redPickups[i]);
                }
                ++i;
            }
            // Must cache here, actions.GetPickupColor().Sum() gonna change after add deposit command
            int carryNum = actions.GetPickupColor().Sum();
            i = 0;
            while (i < pathA.Count)
            {
                if (!actions.GetDepositPos(AIactions).Contains(pathA[i]) && actions.GetDepositPosFromActions(actions).Count < carryNum)
                {
                    actions.MoveTo(pathA[i]);
                    actions.DepositAt(pathA[i], 0);
                }
                ++i;
            }
            return actions;
        }
        else if (GameManager.instance.pathChange == 1)
        {
            if (minCost2 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost2)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
                // Collect all red pickups from generators
                int i = 0;
                while (i < redPickups.Count)
                {
                    if (!actions.GetCollectPos(AIactions).Contains(redPickups[i]) && actions.GetPickupColor().Sum() < GameParameters.instance.carryLimit && actions.GetCollectPos(AIactions).Count < minCost2)
                    {
                        actions.MoveTo(GameManager.instance.parkingPos[Methods.instance.OnPickup(redPickups[i])]);
                        actions.CollectAt(redPickups[i]);
                    }
                    ++i;
                }
                // Must cache here, actions.GetPickupColor().Sum() gonna change after add deposit command
                int carryNum = actions.GetPickupColor().Sum();
                i = 0;
                while (i < pathB.Count)
                {
                    if (!actions.GetDepositPos(AIactions).Contains(pathB[i]) && actions.GetDepositPosFromActions(actions).Count < carryNum)
                    {
                        actions.MoveTo(pathB[i]);
                        actions.DepositAt(pathB[i], 0);
                    }
                    ++i;
                }
                return actions;
            }
        }

        // Randomly choose true path
        //Vector3[] closestAnchorsRed2 = FindCheapestChain(closestAnchorsRed, true);
        int tryNum = Random.Range(0, 3);
        // Choose fake path
        Vector3[] firstAnchors = { GameManager.instance.anchor_a1, GameManager.instance.anchor_a2 };
        Vector3[] SecondAnchors = { GameManager.instance.anchor_a3, GameManager.instance.anchor_a4 };
        //Vector3[] fakeAnchors = FindCheapestChain(trueAnchors, false);
        Debug.Log("first anchors: " + firstAnchors[0] + "  " + firstAnchors[1]);

        GameManager.instance.trueAnchorPos[0] = firstAnchors[0];
        GameManager.instance.trueAnchorPos[1] = firstAnchors[1];
        Debug.Log("second anchors: " + SecondAnchors[0] + "  " + SecondAnchors[1]);

        // Randomly choose generator
        int tryCount = 0;
        while (GameParameters.instance.carryLimit - GetComponent<AIBehavior>().carry.Sum() - actions.GetPickupColor().Sum() > 0)
        {
            tryCount++;
            int generatorId = Random.Range(0, 4);
            tryNum = Random.Range(0, 3);
            if (tryNum <= 1)
            {
                generatorId = Methods.instance.MostRedGenerator();
            }
            List<Vector3> collectList = Methods.instance.PickupsPosInGn(generatorId, GameParameters.instance.carryLimit - GetComponent<AIBehavior>().carry.Sum() - actions.GetPickupColor().Sum());
            collectList = RemovePositionsFromList(collectList, actions.GetCollectPos(AIactions));
            if (collectList.Count > 0)
            {
                actions.MoveTo(GameManager.instance.parkingPos[generatorId]);
                actions.CollectAt(collectList);
            }
            // There is no enough pickup
            if (tryCount > 100) break;
        }

        // Calculate current carry and bag
        int[] LastCarry = new int[3];
        LastCarry = GetComponent<AIBehavior>().GetCarryColor();
        int[] carry = new int[3];
        carry = Methods.instance.PickupColorInPos(actions.paras, GameParameters.instance.carryLimit - GetComponent<AIBehavior>().carry.Sum());
        for (int k = 0; k < 3; k++)
        {
            carry[k] += LastCarry[k];
        }
        int[] bag = actions.GetPickupColorBagPos();

        // Randomly deposit
        Vector3 pos;
        List<int> randomOrder = RandomOrder(actions.GetPickupColor().Sum());
       // List<int> inOrder = InOrder(actions.GetPickupColor().Sum());
        //foreach (int i in inOrder)
        foreach (int i in randomOrder)
        {
            Debug.Log("index: " + i + "   " + "color: " + bag[i]);
            tryNum = Random.Range(0, 3);
            List<Vector3> positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2), true));
            //even
            Debug.Log("the coin is " + tryNum);
                if (bag[i] == 0 && tryNum%2==1)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //Vector3 pos;
                    //path a no plan
                    if (positions.Count == 0)
                    {
                          positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a3), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a4), true));
                    //GameManager.instance.path_current = GameManager.instance.path_b;
                            //pos = Methods.instance.InorderPosition(positions);
                    pos = Methods.instance.RandomPosition(positions);
                    Debug.Log("Deposit change path : "+pos);
                            GameManager.instance.pathChange++;
                    }
                    else
                    {
                    //pos = Methods.instance.InorderPosition(positions);

                    //pos = positions.First();
                    pos = Methods.instance.RandomPosition(positions);
                    GameManager.instance.Total_RealPath_Blocks++;
                        Debug.Log("Deposit real: " + pos);
                    }

                    //Debug.Log("Deposit At First Path: " + pos);

                    if (GameManager.instance.firstBlock == false)
                    {
                        GameManager.instance.Unblocked_Reds++;
                        Debug.Log("Deposit red before the first block:" + GameManager.instance.Unblocked_Reds);
                    }
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));
                }
                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere: " + pos);
                    GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }
            //odd
            if (bag[i] == 0 && tryNum % 2 == 0)
                {
                pos = GetRandomEmptyGrid(AIactions, actions);
                Debug.Log("Deposit red Elsewhere: " + pos);
                GameManager.instance.Total_FalsePath_Blocks++;
                actions.MoveTo(pos);
                actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));
            }
           


            bag[i] = -1;

        }

        return actions;
    }


    public Actions MakeDecisionOne(List<Actions> AIactions, int turn)

    {
        Actions actions = new Actions();
        Debug.Log("now is turn :" + turn);
        Methods.instance.Task1Anchor(GameManager.instance.anchorPositions, out GameManager.instance.Task1_a, out GameManager.instance.Task1_b);

        //Vector3[] trueAnchors;
        Vector3[] closestAnchorsRed = FindCheapestChain(new Vector3[2], true);

        // Check if can win the game this turn
        //List<Vector3> uselessRedCounters = GetUselessRedCounters(closestAnchorsRed);
        List<Vector3> redPickups = GetAllRedPickups();
        int minCost1 = GameManager.instance.Task1_a;
        int minCost2 = GameManager.instance.Task1_b;
        List<Vector3> pathA = GameManager.instance.path_current;
        List<Vector3> pathB = GameManager.instance.path_b;
        Debug.Log("minCost1: " + minCost1 + "  " + "minCost2 :" + minCost2 + "  ");
        //"useless: " + uselessRedCounters.Count);
        if (minCost1 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost1 && GameManager.instance.pathChange == 0)
        //+uselessRedCounters.Count >= minCost)
        {
            //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
            // Collect all red pickups from generators
            int i = 0;
            while (i < redPickups.Count)
            {
                if (!actions.GetCollectPos(AIactions).Contains(redPickups[i]) && actions.GetPickupColor().Sum() < GameParameters.instance.carryLimit && actions.GetCollectPos(AIactions).Count < minCost1)
                {
                    actions.MoveTo(GameManager.instance.parkingPos[Methods.instance.OnPickup(redPickups[i])]);
                    actions.CollectAt(redPickups[i]);
                }
                ++i;
            }
            // Must cache here, actions.GetPickupColor().Sum() gonna change after add deposit command
            int carryNum = actions.GetPickupColor().Sum();
            i = 0;
            while (i < pathA.Count)
            {
                if (!actions.GetDepositPos(AIactions).Contains(pathA[i]) && actions.GetDepositPosFromActions(actions).Count < carryNum)
                {
                    actions.MoveTo(pathA[i]);
                    actions.DepositAt(pathA[i], 0);
                }
                ++i;
            }
            return actions;
        }
        else if (GameManager.instance.pathChange == 1)
        {
            if (minCost2 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost2)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
                // Collect all red pickups from generators
                int i = 0;
                while (i < redPickups.Count)
                {
                    if (!actions.GetCollectPos(AIactions).Contains(redPickups[i]) && actions.GetPickupColor().Sum() < GameParameters.instance.carryLimit && actions.GetCollectPos(AIactions).Count < minCost2)
                    {
                        actions.MoveTo(GameManager.instance.parkingPos[Methods.instance.OnPickup(redPickups[i])]);
                        actions.CollectAt(redPickups[i]);
                    }
                    ++i;
                }
                // Must cache here, actions.GetPickupColor().Sum() gonna change after add deposit command
                int carryNum = actions.GetPickupColor().Sum();
                i = 0;
                while (i < pathB.Count)
                {
                    if (!actions.GetDepositPos(AIactions).Contains(pathB[i]) && actions.GetDepositPosFromActions(actions).Count < carryNum)
                    {
                        actions.MoveTo(pathB[i]);
                        actions.DepositAt(pathB[i], 0);
                    }
                    ++i;
                }
                return actions;
            }
        }

        // Randomly choose true path
        //Vector3[] closestAnchorsRed2 = FindCheapestChain(closestAnchorsRed, true);
        int tryNum = Random.Range(0, 3);
        // Choose fake path
        Vector3[] firstAnchors = { GameManager.instance.anchor_a1, GameManager.instance.anchor_a2 };
        Vector3[] SecondAnchors = { GameManager.instance.anchor_a3, GameManager.instance.anchor_a4 };
        //Vector3[] fakeAnchors = FindCheapestChain(trueAnchors, false);
        Debug.Log("first anchors: " + firstAnchors[0] + "  " + firstAnchors[1]);

        GameManager.instance.trueAnchorPos[0] = firstAnchors[0];
        GameManager.instance.trueAnchorPos[1] = firstAnchors[1];
        Debug.Log("second anchors: " + SecondAnchors[0] + "  " + SecondAnchors[1]);

        // Randomly choose generator
        int tryCount = 0;
        while (GameParameters.instance.carryLimit - GetComponent<AIBehavior>().carry.Sum() - actions.GetPickupColor().Sum() > 0)
        {
            tryCount++;
            int generatorId = Random.Range(0, 4);
            tryNum = Random.Range(0, 3);
            if (tryNum <= 1)
            {
                generatorId = Methods.instance.MostRedGenerator();
            }
            List<Vector3> collectList = Methods.instance.PickupsPosInGn(generatorId, GameParameters.instance.carryLimit - GetComponent<AIBehavior>().carry.Sum() - actions.GetPickupColor().Sum());
            collectList = RemovePositionsFromList(collectList, actions.GetCollectPos(AIactions));
            if (collectList.Count > 0)
            {
                actions.MoveTo(GameManager.instance.parkingPos[generatorId]);
                actions.CollectAt(collectList);
            }
            // There is no enough pickup
            if (tryCount > 100) break;
        }

        // Calculate current carry and bag
        int[] LastCarry = new int[3];
        LastCarry = GetComponent<AIBehavior>().GetCarryColor();
        int[] carry = new int[3];
        carry = Methods.instance.PickupColorInPos(actions.paras, GameParameters.instance.carryLimit - GetComponent<AIBehavior>().carry.Sum());
        for (int k = 0; k < 3; k++)
        {
            carry[k] += LastCarry[k];
        }
        int[] bag = actions.GetPickupColorBagPos();

        // Randomly deposit
        Vector3 pos;
        //List<int> randomOrder = RandomOrder(actions.GetPickupColor().Sum());
        List<int> inOrder = InOrder(actions.GetPickupColor().Sum());
        foreach (int i in inOrder)
        {
            Debug.Log("index: " + i + "   " + "color: " + bag[i]);
            //tryNum = Random.Range(0, 3);
            List<Vector3> positions = GameManager.instance.path_current;
            if (turn == 0)
            {
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //Vector3 pos;
                    if (positions.Count == 0)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere: " + pos);
                        GameManager.instance.Total_FalsePath_Blocks++;
                    }
                    else
                    {
                        pos = Methods.instance.InorderPosition(positions);
                        //pos = positions.First();
                        //Methods.instance.RandomPosition(positions);
                        GameManager.instance.Total_RealPath_Blocks++;
                        Debug.Log("Deposit real: " + pos);
                    }

                    //Debug.Log("Deposit At First Path: " + pos);

                    if (GameManager.instance.firstBlock == false)
                    {
                        GameManager.instance.Unblocked_Reds++;
                        Debug.Log("Deposit red before the first block:" + GameManager.instance.Unblocked_Reds);
                    }
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));
                }
                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere: " + pos);
                    GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }
                //try to find if be blocked
                //else if (bag[i] == 0 && turn != 0)
                //{
                //    foreach (Vector3 blocks in GameManager.instance.blockedTile)
                //    {
                //        if (positions.Contains(blocks) == true)
                //        {
                //            positions = GameManager.instance.path_b;
                //        }
                //    }
                //}
            }
            else if (turn != 0)
            {
                //if block the path a change to b
                if (GameManager.instance.firstBlock == true)
                {
                    positions = GameManager.instance.path_b;
                    GameManager.instance.path_current = GameManager.instance.path_b;
                    Debug.Log("Deposit change path : ");
                    GameManager.instance.pathChange++;
                }
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //Vector3 pos;
                    //foreach (Vector3 ooo in positions)
                    //{
                    //    Debug.Log("??????" + ooo);
                    //}
                    //pos = positions.First();
                    pos = Methods.instance.InorderPosition(positions);
                    //Debug.Log("Deposit At First Path: " + pos);
                    GameManager.instance.Total_RealPath_Blocks++;
                    if (GameManager.instance.firstBlock == false)
                    {
                        GameManager.instance.Unblocked_Reds++;
                        Debug.Log("Deposit red before the first block:" + GameManager.instance.Unblocked_Reds);
                    }
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));
                }
                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere: " + pos);
                    GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));

                }
            }



            bag[i] = -1;

        }

        return actions;
    }
}

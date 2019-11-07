/*
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
        //Methods.instance.Task1Anchor(GameManager.instance.anchorPositions, out GameManager.instance.Task1_a, out GameManager.instance.Task1_b);

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
            //imete as coin
            tryNum = Random.Range(0, 3);
            List<Vector3> positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2), true));
            GameManager.instance.path_current = positions;


            if (GameManager.instance.pathChange == 0)
            {
                if (bag[i] == 0 && tryNum % 2 == 1)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //positions = GameManager.instance.path_current;


                    //Vector3 pos;
                    //if no path a(out of the narrative region),change to path b

                    //pos = Methods.instance.InorderPosition(positions);
                    Debug.Log(" the posistion is " + positions.Count);
                    //!!!!! current nothing



                    if (Methods.instance.Contains(Methods.instance.non_contiguous(positions, turn, minCost1, GameManager.instance.depositRed), firstAnchors) == false)
                    {
                        //pos = GetRandomEmptyGrid(AIactions, actions);
                        //Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        positions = GameManager.instance.path_b;
                        GameManager.instance.path_current = GameManager.instance.path_b;
                        
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        //pos = Methods.instance.non_contiguous(positions, turn,minCost2, GameManager.instance.depositRed);
                        Debug.Log("Deposit path b cause no path a: " + pos);
                        //GameManager.instance.Real_Path_Replan = turn + 1;
                        //GameManager.instance.gameLog += "the replaned time" + GameManager.instance.Real_Path_Replan + "\n";


                    }
                    else
                    {
                        pos = Methods.instance.non_contiguous(positions, turn, minCost1, GameManager.instance.depositRed);

                        Debug.Log("Deposit real: " + pos);
                    }
                    //if (GameManager.instance.firstBlock == false)
                    //{
                    //    GameManager.instance.Unblocked_Reds++;
                    //    Debug.Log("Deposit red before the first block:" + GameManager.instance.Unblocked_Reds);
                    //}
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }
                else if (bag[i] == 0 && tryNum % 2 == 0)
                {
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit red Elsewhere: " + pos);
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));
                }

                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }
            else if (GameManager.instance.pathChange == 1)
            {
                positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a3), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a4), true));
                GameManager.instance.path_current = positions;
                if (bag[i] == 0&&tryNum % 2 == 1)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //Vector3 pos;
                    //if no path a(out of the narrative region), stop change
                    if (Methods.instance.Contains(Methods.instance.non_contiguous(positions, turn, minCost2, GameManager.instance.depositRed), SecondAnchors) == false)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        //turn = 0;
                    }
                    else
                    {
                        pos = Methods.instance.non_contiguous(positions, turn, minCost2, GameManager.instance.depositRed);

                        //GameManager.instance.Total_RealPath_Blocks++;
                        Debug.Log("Deposit real: " + pos);
                    }

                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }
                else if (bag[i] == 0 && tryNum % 2 == 0)
                {
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit red Elsewhere: " + pos);
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));
                }

                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere cause no red token: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }

            bag[i] = -1;

        }

        return actions;
    }


    public Actions MakeDecisionOne(List<Actions> AIactions, int turn)
    {
        
            Actions actions = new Actions();
            Debug.Log("now is turn :" + turn);
           
            List<Vector3> redPickups = GetAllRedPickups();
            int minCost1 = GameManager.instance.Task1_a;
            int minCost2 = GameManager.instance.Task1_b;
        List<Vector3> pathA = GameManager.instance.path_a;
        List<Vector3> pathB = GameManager.instance.path_b;
        Debug.Log(" the path a is " + pathA.Count);
        Debug.Log(" the path b is " + pathB.Count);
        Debug.Log("minCost1: " + minCost1 + "  " + "minCost2 :" + minCost2 + "  ");
        if(GameManager.instance.pathChange == 0)
        {

            if (minCost1 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost1 && GameManager.instance.pathChange == 0)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
                // Collect all red pickups from generators
                //GameManager.instance.path_current = pathA;

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
        }
        //ALL path a not avaliable, change to path B
        else if (GameManager.instance.pathChange == 1)
        {
            if (minCost2 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost2)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
                //GameManager.instance.path_current = pathB;
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

            // in order choose true path
            //Vector3[] closestAnchorsRed2 = FindCheapestChain(closestAnchorsRed, true);
            int tryNum = Random.Range(0, 3);
            // Choose fake path
            Vector3[] firstAnchors = { GameManager.instance.anchor_a1, GameManager.instance.anchor_a2 };
            Vector3[] secondAnchors = { GameManager.instance.anchor_a3, GameManager.instance.anchor_a4 };
            //Vector3[] fakeAnchors = FindCheapestChain(trueAnchors, false);
            Debug.Log("first anchors: " + firstAnchors[0] + "  " + firstAnchors[1]);

            GameManager.instance.trueAnchorPos[0] = firstAnchors[0];
            GameManager.instance.trueAnchorPos[1] = firstAnchors[1];
            Debug.Log("second anchors: " + secondAnchors[0] + "  " + secondAnchors[1]);

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

            // in order deposit
            Vector3 pos;
            //List<int> randomOrder = RandomOrder(actions.GetPickupColor().Sum());
            List<int> inOrder = InOrder(actions.GetPickupColor().Sum());
            foreach (int i in inOrder)
            {
                Debug.Log("index: " + i + "   " + "color: " + bag[i]);
            //tryNum = Random.Range(0, 3);
            List<Vector3> positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2), true));
            //List<Vector3> positions = GameManager.instance.path_current;
            GameManager.instance.path_current = positions;
            


            //change according to the pathChange para
            if (GameManager.instance.pathChange==0)
            {
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
      



                
                    //if no path a(out of the narrative region),change to path b

                    //pos = Methods.instance.InorderPosition(positions);
                    Debug.Log(" the posistion is " +positions.Count);
                    //!!!!! current nothing



                    if (Methods.instance.Contains(Methods.instance.InorderPosition(positions), firstAnchors) == false)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        positions = GameManager.instance.path_b;
                        GameManager.instance.path_current = GameManager.instance.path_b;
                        //GameManager.instance.Real_Path_Replan = turn + 1;
                        //GameManager.instance.gameLog += "the replaned time" + GameManager.instance.Real_Path_Replan +"\n";

                    }
                    else
                    {

                        pos = Methods.instance.InorderPosition(positions);
                      
                        Debug.Log("Deposit real: " + pos);
                    }

                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }

                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }
                
            }
            else if (GameManager.instance.pathChange==1)
            {
                positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a3), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a4), true));
                GameManager.instance.path_current = positions;
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //Vector3 pos;
                    //if no path a(out of the narrative region), stop change
                    if (Methods.instance.Contains(Methods.instance.InorderPosition(positions), secondAnchors) == false)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        GameManager.instance.pathChange++;
                        //GameManager.instance.Real_Path_Replan = turn + 1 - GameManager.instance.Real_Path_Replan;
                        //Debug.Log("the replaned time: " + GameManager.instance.Real_Path_Replan + " \n");
                        //GameManager.instance.gameLog += "the replaned time: " + GameManager.instance.Real_Path_Replan+ " \n";
                    }
                    else
                    {
                        pos = Methods.instance.InorderPosition(positions);
                   
                        Debug.Log("Deposit real: " + pos);
                    }
                   
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }

                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere cause no red token: " + pos);
  
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));
                }

            }


                bag[i] = -1;

            }

            return actions;
        }




    // in order but not start from anchor
    public Actions MakeDecisionsTwo(List<Actions> AIactions, int turn)
    {

        Actions actions = new Actions();
        Debug.Log("now is ai 2 :" + turn);

        Methods.instance.Task1Anchor(GameManager.instance.anchorPositions, out GameManager.instance.Task1_a, out GameManager.instance.Task1_b);

        //Vector3[] trueAnchors;
        Vector3[] closestAnchorsRed = FindCheapestChain(new Vector3[2], true);

        // Check if can win the game this turn
        //List<Vector3> uselessRedCounters = GetUselessRedCounters(closestAnchorsRed);
        List<Vector3> redPickups = GetAllRedPickups();
        int minCost1 = GameManager.instance.Task1_a;
        int minCost2 = GameManager.instance.Task1_b;
        List<Vector3> pathA = GameManager.instance.path_a;
        //List<Vector3> pathA= Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(GameManager.instance.anchor_a1, GameManager.instance.anchor_a2, true));
        //List<Vector3> pathB= Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(GameManager.instance.anchor_a3, GameManager.instance.anchor_a4, true));
        List<Vector3> pathB = GameManager.instance.path_b;
        Debug.Log(" the path a is " + pathA.Count);
        Debug.Log(" the path a is " + pathB.Count);
        //List<Vector3> pathB = GameManager.instance.path_b;
        Debug.Log("minCost1: " + minCost1 + "  " + "minCost2 :" + minCost2 + "  ");
        //"useless: " + uselessRedCounters.Count);
        if (GameManager.instance.pathChange == 0)
        {

            if (minCost1 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost1 && GameManager.instance.pathChange == 0)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
                // Collect all red pickups from generators
                //GameManager.instance.path_current = pathA;

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
        }
        //ALL path a not avaliable, change to path B
        else if (GameManager.instance.pathChange == 1)
        {
            if (minCost2 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost2)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
               // GameManager.instance.path_current = pathB;
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

        // in order choose true path
        //Vector3[] closestAnchorsRed2 = FindCheapestChain(closestAnchorsRed, true);
        int tryNum = Random.Range(0, 3);
        // Choose fake path
        Vector3[] firstAnchors = { GameManager.instance.anchor_a1, GameManager.instance.anchor_a2 };
        Vector3[] secondAnchors = { GameManager.instance.anchor_a3, GameManager.instance.anchor_a4 };
        //Vector3[] fakeAnchors = FindCheapestChain(trueAnchors, false);
        Debug.Log("first anchors: " + firstAnchors[0] + "  " + firstAnchors[1]);

        GameManager.instance.trueAnchorPos[0] = firstAnchors[0];
        GameManager.instance.trueAnchorPos[1] = firstAnchors[1];
        Debug.Log("second anchors: " + secondAnchors[0] + "  " + secondAnchors[1]);

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

        // in order deposit
        Vector3 pos;
        List<int> inOrder = InOrder(actions.GetPickupColor().Sum());
        foreach (int i in inOrder)
        {
            Debug.Log("index: " + i + "   " + "color: " + bag[i]);
            //tryNum = Random.Range(0, 3);
            List<Vector3> positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2), true));

            GameManager.instance.path_current = positions;
            //change according to the pathChange para
            if (GameManager.instance.pathChange == 0)
            {
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //positions = GameManager.instance.path_current;


                    //Vector3 pos;
                    //if no path a(out of the narrative region),change to path b

                    //pos = Methods.instance.InorderPosition(positions);
                    Debug.Log(" the posistion is " + positions.Count);
                    //!!!!! current nothing



                    if (Methods.instance.Contains(Methods.instance.Random_InoderPosition(positions,turn,GameManager.instance.depositRed), firstAnchors) == false)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        positions = GameManager.instance.path_b;
                        //GameManager.instance.path_current = GameManager.instance.path_b;
                        GameManager.instance.path_current = GameManager.instance.path_b;
                        //GameManager.instance.Real_Path_Replan = turn + 1;
                        //GameManager.instance.gameLog += "the replaned time" + GameManager.instance.Real_Path_Replan + "\n";


                    }
                    else
                    {
                        pos = Methods.instance.Random_InoderPosition(positions,turn, GameManager.instance.depositRed);
                        
                        Debug.Log("Deposit real: " + pos);
                    }
                    //if (GameManager.instance.firstBlock == false)
                    //{
                    //    GameManager.instance.Unblocked_Reds++;
                    //    Debug.Log("Deposit red before the first block:" + GameManager.instance.Unblocked_Reds);
                    //}
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }

                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }
            else if (GameManager.instance.pathChange == 1)
            {
                positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a3), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a4), true));
                GameManager.instance.path_current = positions;
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //Vector3 pos;
                    //if no path a(out of the narrative region), stop change
                    if (Methods.instance.Contains(Methods.instance.Random_InoderPosition(positions,turn,GameManager.instance.depositRed), secondAnchors) == false)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        //GameManager.instance.Real_Path_Replan = turn + 1 - GameManager.instance.Real_Path_Replan;
                        //Debug.Log("the replaned time: " + GameManager.instance.Real_Path_Replan + " \n");
                        //GameManager.instance.gameLog += "the replaned time: " + GameManager.instance.Real_Path_Replan + " \n";

                       
                    }
                    else
                    {
                        pos = Methods.instance.Random_InoderPosition(positions,turn, GameManager.instance.depositRed);


                        Debug.Log("Deposit real: " + pos);
                    }

                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }

                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere cause no red token: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }


            bag[i] = -1;

        }

        return actions;
    }



    public Actions MakeDecisionsThree(List<Actions> AIactions, int turn)
    {

        Actions actions = new Actions();
        Debug.Log("now is ai 3 :" + turn);

        Methods.instance.Task1Anchor(GameManager.instance.anchorPositions, out GameManager.instance.Task1_a, out GameManager.instance.Task1_b);

        //Vector3[] trueAnchors;
        Vector3[] closestAnchorsRed = FindCheapestChain(new Vector3[2], true);

        // Check if can win the game this turn
        //List<Vector3> uselessRedCounters = GetUselessRedCounters(closestAnchorsRed);
        List<Vector3> redPickups = GetAllRedPickups();
        int minCost1 = GameManager.instance.Task1_a;
        int minCost2 = GameManager.instance.Task1_b;
        List<Vector3> pathA = GameManager.instance.path_a;
        //List<Vector3> pathA= Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(GameManager.instance.anchor_a1, GameManager.instance.anchor_a2, true));
        //List<Vector3> pathB= Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(GameManager.instance.anchor_a3, GameManager.instance.anchor_a4, true));
        List<Vector3> pathB = GameManager.instance.path_b;
        Debug.Log(" the path a is " + pathA.Count);
        Debug.Log(" the path B is " + pathB.Count);
        //List<Vector3> pathB = GameManager.instance.path_b;
        Debug.Log("minCost1: " + minCost1 + "  " + "minCost2 :" + minCost2 + "  ");
        //"useless: " + uselessRedCounters.Count);
        if (GameManager.instance.pathChange == 0)
        {

            if (minCost1 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost1 && GameManager.instance.pathChange == 0)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
                // Collect all red pickups from generators
                GameManager.instance.path_current = pathA;

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
        }
        //ALL path a not avaliable, change to path B
        else if (GameManager.instance.pathChange == 1)
        {
            if (minCost2 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost2)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
                //GameManager.instance.path_current = pathB;
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

        // in order choose true path
        //Vector3[] closestAnchorsRed2 = FindCheapestChain(closestAnchorsRed, true);
        int tryNum = Random.Range(0, 3);
        // Choose fake path
        Vector3[] firstAnchors = { GameManager.instance.anchor_a1, GameManager.instance.anchor_a2 };
        Vector3[] secondAnchors = { GameManager.instance.anchor_a3, GameManager.instance.anchor_a4 };
        //Vector3[] fakeAnchors = FindCheapestChain(trueAnchors, false);
        Debug.Log("first anchors: " + firstAnchors[0] + "  " + firstAnchors[1]);

        GameManager.instance.trueAnchorPos[0] = firstAnchors[0];
        GameManager.instance.trueAnchorPos[1] = firstAnchors[1];
        Debug.Log("second anchors: " + secondAnchors[0] + "  " + secondAnchors[1]);

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

        // in order deposit
        Vector3 pos;
        //List<int> randomOrder = RandomOrder(actions.GetPickupColor().Sum());
        List<int> inOrder = InOrder(actions.GetPickupColor().Sum());
        foreach (int i in inOrder)
        {
            Debug.Log("index: " + i + "   " + "color: " + bag[i]);
            //tryNum = Random.Range(0, 3);
            List<Vector3> positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2), true));
            GameManager.instance.path_current = positions;
            //change according to the pathChange para
            if (GameManager.instance.pathChange == 0)
            {
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //positions = GameManager.instance.path_current;


                    //Vector3 pos;
                    //if no path a(out of the narrative region),change to path b

                    //pos = Methods.instance.InorderPosition(positions);
                    Debug.Log(" the posistion is " + positions.Count);
                    //!!!!! current nothing



                    if (Methods.instance.Contains(Methods.instance.non_contiguous(positions, turn,minCost1, GameManager.instance.depositRed), firstAnchors) == false)
                    {
                        //pos = GetRandomEmptyGrid(AIactions, actions);
                        //Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        positions = GameManager.instance.path_b;
                        GameManager.instance.path_current = GameManager.instance.path_b;
                        //??????
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        //pos = Methods.instance.non_contiguous(positions, turn,minCost2, GameManager.instance.depositRed);
                        Debug.Log("Deposit path b cause no path a: " + pos);
                        //GameManager.instance.Real_Path_Replan = turn + 1;
                        //GameManager.instance.gameLog += "the replaned time" + GameManager.instance.Real_Path_Replan + "\n";


                    }
                    else
                    {
                        pos = Methods.instance.non_contiguous(positions, turn,minCost1, GameManager.instance.depositRed);
                       
                        Debug.Log("Deposit real: " + pos);
                    }
                    //if (GameManager.instance.firstBlock == false)
                    //{
                    //    GameManager.instance.Unblocked_Reds++;
                    //    Debug.Log("Deposit red before the first block:" + GameManager.instance.Unblocked_Reds);
                    //}
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }

                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }
            else if (GameManager.instance.pathChange == 1)
            {
                positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a3), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a4), true));
                GameManager.instance.path_current = positions;
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //Vector3 pos;
                    //if no path a(out of the narrative region), stop change
                    if (Methods.instance.Contains(Methods.instance.non_contiguous(positions, turn,minCost2, GameManager.instance.depositRed), secondAnchors) == false)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        //turn = 0;
                    }
                    else
                    {
                        pos = Methods.instance.non_contiguous(positions, turn,minCost2, GameManager.instance.depositRed);

                        //GameManager.instance.Total_RealPath_Blocks++;
                        Debug.Log("Deposit real: " + pos);
                    }

                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }

                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere cause no red token: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }


            bag[i] = -1;

        }

        return actions;
    }


   
    public Actions MakeDecisionsFour(List<Actions> AIactions, int turn)
    {

        Actions actions = new Actions();
        Debug.Log("now is ai 4 :" + turn);

        Methods.instance.Task1Anchor(GameManager.instance.anchorPositions, out GameManager.instance.Task1_a, out GameManager.instance.Task1_b);

        //Vector3[] trueAnchors;
        Vector3[] closestAnchorsRed = FindCheapestChain(new Vector3[2], true);

        // Check if can win the game this turn
        //List<Vector3> uselessRedCounters = GetUselessRedCounters(closestAnchorsRed);
        List<Vector3> redPickups = GetAllRedPickups();
        int minCost1 = GameManager.instance.Task1_a;
        int minCost2 = GameManager.instance.Task1_b;
        List<Vector3> pathA = GameManager.instance.path_a;
        //List<Vector3> pathA= Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(GameManager.instance.anchor_a1, GameManager.instance.anchor_a2, true));
        //List<Vector3> pathB= Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(GameManager.instance.anchor_a3, GameManager.instance.anchor_a4, true));
        List<Vector3> pathB = GameManager.instance.path_b;
        Debug.Log(" the path a is " + pathA.Count);
        Debug.Log(" the path a is " + pathB.Count);
        //List<Vector3> pathB = GameManager.instance.path_b;
        Debug.Log("minCost1: " + minCost1 + "  " + "minCost2 :" + minCost2 + "  ");
        //"useless: " + uselessRedCounters.Count);
        if (GameManager.instance.pathChange == 0)
        {

            if (minCost1 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost1 && GameManager.instance.pathChange == 0)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
                // Collect all red pickups from generators
                GameManager.instance.path_current = pathA;

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
        }
        //ALL path a not avaliable, change to path B
        else if (GameManager.instance.pathChange == 1)
        {
            if (minCost2 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost2)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
                GameManager.instance.path_current = pathB;
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

        // in order choose true path
        //Vector3[] closestAnchorsRed2 = FindCheapestChain(closestAnchorsRed, true);
        int tryNum = Random.Range(0, 3);
        // Choose fake path
        Vector3[] firstAnchors = { GameManager.instance.anchor_a1, GameManager.instance.anchor_a2 };
        Vector3[] secondAnchors = { GameManager.instance.anchor_a3, GameManager.instance.anchor_a4 };
        //Vector3[] fakeAnchors = FindCheapestChain(trueAnchors, false);
        Debug.Log("first anchors: " + firstAnchors[0] + "  " + firstAnchors[1]);

        GameManager.instance.trueAnchorPos[0] = firstAnchors[0];
        GameManager.instance.trueAnchorPos[1] = firstAnchors[1];
        Debug.Log("second anchors: " + secondAnchors[0] + "  " + secondAnchors[1]);

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

        // in order deposit
        Vector3 pos;
        //List<int> randomOrder = RandomOrder(actions.GetPickupColor().Sum());
        List<int> inOrder = InOrder(actions.GetPickupColor().Sum());
        foreach (int i in inOrder)
        {
            Debug.Log("index: " + i + "   " + "color: " + bag[i]);
            //tryNum = Random.Range(0, 3);
            List<Vector3> positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2), true));
            List<Vector3> fakeposition= Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a3), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a4), true));
            //change according to the pathChange para
            if (GameManager.instance.pathChange == 0)
            {
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    fakeposition = RemovePositionsFromList(fakeposition,actions.GetDepositPos(AIactions));
                    GameManager.instance.path_b = fakeposition;
                    //positions = GameManager.instance.path_current;
                    GameManager.instance.path_current = positions;
                    //Vector3 pos;
                    //if no path a(out of the narrative region),change to path b

                    //pos = Methods.instance.InorderPosition(positions);
                    Debug.Log(" the posistion is " + positions.Count);
                    //!!!!! current nothing



                    if (Methods.instance.Contains(Methods.instance.non_contiguous(positions, turn,minCost1, GameManager.instance.depositRed), firstAnchors) == false)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        positions = GameManager.instance.path_b;
                        GameManager.instance.path_current = GameManager.instance.path_b;
                        //??????
                        //pos = Methods.instance.non_contiguous(positions, turn, GameManager.instance.depositRed);
                        //Debug.Log("Deposit path b cause no path a: " + pos);

                    }
                    else
                    {
                        pos = Methods.instance.non_contiguous(positions, turn,minCost1, GameManager.instance.depositRed);
                        //pos = positions.First();
                        //Methods.instance.RandomPosition(positions);
                        //GameManager.instance.Total_RealPath_Blocks++;
                        Debug.Log("Deposit real: " + pos);
                    }
                    //if (GameManager.instance.firstBlock == false)
                    //{
                    //    GameManager.instance.Unblocked_Reds++;
                    //    Debug.Log("Deposit red before the first block:" + GameManager.instance.Unblocked_Reds);
                    //}
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }

                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    pos = Methods.instance.non_contiguous_nonRed(fakeposition);
                    Debug.Log("Deposit path b: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }
            else if (GameManager.instance.pathChange == 1)
            {
                positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a3), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a4), true));
                //fakeposition = RemovePositionsFromList(fakeposition, actions.GetDepositPos(AIactions));
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    GameManager.instance.path_current = positions;
                    //Vector3 pos;
                    //if no path a(out of the narrative region), stop change
                    if (Methods.instance.Contains(Methods.instance.non_contiguous(positions, turn,minCost2, GameManager.instance.depositRed), secondAnchors) == false)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        turn = 0;
                    }
                    else
                    {
                        pos = Methods.instance.non_contiguous(positions, turn,minCost2, GameManager.instance.depositRed);

                        //GameManager.instance.Total_RealPath_Blocks++;
                        Debug.Log("Deposit real: " + pos);
                    }

                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }

                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere cause no FAKE : " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }


            bag[i] = -1;

        }

        return actions;
    }

    //five random deposit in a3, a4 narrative region
    public Actions MakeDecisionsFive(List<Actions> AIactions, int turn)
    {

        Actions actions = new Actions();
        Debug.Log("now is ai 5 :" + turn);

        //Methods.instance.Task1Anchor(GameManager.instance.anchorPositions, out GameManager.instance.Task1_a, out GameManager.instance.Task1_b);

        //Vector3[] trueAnchors;
        Vector3[] closestAnchorsRed = FindCheapestChain(new Vector3[2], true);

        // Check if can win the game this turn
        //List<Vector3> uselessRedCounters = GetUselessRedCounters(closestAnchorsRed);
        List<Vector3> redPickups = GetAllRedPickups();
        int minCost1 = GameManager.instance.Task1_a;
        int minCost2 = GameManager.instance.Task1_b;
        List<Vector3> pathA = GameManager.instance.path_a;
        //List<Vector3> pathA= Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(GameManager.instance.anchor_a1, GameManager.instance.anchor_a2, true));
        //List<Vector3> pathB= Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(GameManager.instance.anchor_a3, GameManager.instance.anchor_a4, true));
        List<Vector3> pathB = GameManager.instance.path_b;
        Debug.Log(" the path a is " + pathA.Count);
        Debug.Log(" the path b is " + pathB.Count);
        //List<Vector3> pathB = GameManager.instance.path_b;
        Debug.Log("minCost1: " + minCost1 + "  " + "minCost2 :" + minCost2 + "  ");
        //"useless: " + uselessRedCounters.Count);
        if (GameManager.instance.pathChange == 0)
        {

            if (minCost1 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost1 && GameManager.instance.pathChange == 0)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
                // Collect all red pickups from generators
                //GameManager.instance.path_current = pathA;

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
        }
        //ALL path a not avaliable, change to path B
        else if (GameManager.instance.pathChange == 1)
        {
            if (minCost2 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost2)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
                //GameManager.instance.path_current = pathB;
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

        // in order choose true path
        //Vector3[] closestAnchorsRed2 = FindCheapestChain(closestAnchorsRed, true);
        int tryNum = Random.Range(0, 3);
        // Choose fake path
        Vector3[] firstAnchors = { GameManager.instance.anchor_a1, GameManager.instance.anchor_a2 };
        Vector3[] secondAnchors = { GameManager.instance.anchor_a3, GameManager.instance.anchor_a4 };
        //Vector3[] fakeAnchors = FindCheapestChain(trueAnchors, false);
        Debug.Log("first anchors: " + firstAnchors[0] + "  " + firstAnchors[1]);

        GameManager.instance.trueAnchorPos[0] = firstAnchors[0];
        GameManager.instance.trueAnchorPos[1] = firstAnchors[1];
        Debug.Log("second anchors: " + secondAnchors[0] + "  " + secondAnchors[1]);

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

        // in order deposit
        Vector3 pos;
        //List<int> randomOrder = RandomOrder(actions.GetPickupColor().Sum());
        List<int> inOrder = InOrder(actions.GetPickupColor().Sum());
        foreach (int i in inOrder)
        {
            Debug.Log("index: " + i + "   " + "color: " + bag[i]);
            //tryNum = Random.Range(0, 3);
            List<Vector3> positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2), true));

            //change according to the pathChange para
            if (GameManager.instance.pathChange == 0)
            {
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                     GameManager.instance.path_current=positions;


                    //Vector3 pos;
                    //if no path a(out of the narrative region),change to path b

                    //pos = Methods.instance.InorderPosition(positions);
                    Debug.Log(" the posistion is " + positions.Count);
                    //!!!!! current nothing



                    if (Methods.instance.Contains(Methods.instance.non_contiguous(positions, turn, minCost1,GameManager.instance.depositRed), firstAnchors) == false)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere cause no a path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        positions = GameManager.instance.path_b;
                        GameManager.instance.path_current = GameManager.instance.path_b;
                        //??????
                        //pos = Methods.instance.non_contiguous(positions, turn, GameManager.instance.depositRed);
                        //Debug.Log("Deposit path b cause no path a: " + pos);

                    }
                    else
                    {
                        pos = Methods.instance.non_contiguous(positions, turn,minCost1, GameManager.instance.depositRed);
                        
                        Debug.Log("Deposit real: " + pos);
                    }
                    //if (GameManager.instance.firstBlock == false)
                    //{
                    //    GameManager.instance.Unblocked_Reds++;
                    //    Debug.Log("Deposit red before the first block:" + GameManager.instance.Unblocked_Reds);
                    //}
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }

                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    if (Methods.instance.Contains(Methods.instance.non_contiguous(positions, turn, minCost2, GameManager.instance.depositRed), secondAnchors) == false)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        turn = 0;
                    }
                    else
                    {
                        pos = Methods.instance.non_contiguous(positions, turn, minCost2, GameManager.instance.depositRed);

                        //GameManager.instance.Total_RealPath_Blocks++;
                        Debug.Log("Deposit real: " + pos);
                    }
                    Debug.Log("Deposit Elsewhere: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }
            else if (GameManager.instance.pathChange == 1)
            {
                positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a3), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a4), true));
                GameManager.instance.path_current = positions;
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    GameManager.instance.path_current = positions;
                    //Vector3 pos;
                    //if no path a(out of the narrative region), stop change
                    if (Methods.instance.Contains(Methods.instance.non_contiguous(positions, turn,minCost2, GameManager.instance.depositRed), secondAnchors) == false)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        turn = 0;
                    }
                    else
                    {
                        pos = Methods.instance.non_contiguous(positions, turn,minCost2, GameManager.instance.depositRed);

                        //GameManager.instance.Total_RealPath_Blocks++;
                        Debug.Log("Deposit real: " + pos);
                    }

                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }

                else if (bag[i] != 0)
                {
                    pos = GetRandomEmptyGrid(AIactions, actions);
                    //Vector3 pos;
                    //pos = Methods.instance.RandomPosition(Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a3), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a4)));
                    Debug.Log("Deposit Elsewhere cause no red token: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }


            bag[i] = -1;

        }

        return actions;
    }

    //six in task2 test1
    public Actions MakeDecisionsSix(List<Actions> AIactions, int turn)
    {

        Actions actions = new Actions();
        Debug.Log("now is ai 6 :" + turn);
        Methods.instance.Task2Set();
        

        Debug.Log("the overlap is: " + Methods.instance.CalculateOverlap(GameManager.instance.anchor_a1, GameManager.instance.anchor_a2, GameManager.instance.anchor_a3, GameManager.instance.anchor_a4));

        //Methods.instance.Task1Anchor(GameManager.instance.anchorPositions, out GameManager.instance.Task1_a, out GameManager.instance.Task1_b);

        //Vector3[] trueAnchors;
        Vector3[] closestAnchorsRed = FindCheapestChain(new Vector3[2], true);

        // Check if can win the game this turn
        //List<Vector3> uselessRedCounters = GetUselessRedCounters(closestAnchorsRed);
        List<Vector3> redPickups = GetAllRedPickups();
        int minCost1 = GameManager.instance.Task1_a;
        int minCost2 = GameManager.instance.Task1_b;
        List<Vector3> pathA = GameManager.instance.path_a;
        //List<Vector3> pathA= Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(GameManager.instance.anchor_a1, GameManager.instance.anchor_a2, true));
        //List<Vector3> pathB= Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(GameManager.instance.anchor_a3, GameManager.instance.anchor_a4, true));
        List<Vector3> pathB = GameManager.instance.path_b;
        Debug.Log(" the path a is " + pathA.Count);
        Debug.Log(" the path a is " + pathB.Count);
        //List<Vector3> pathB = GameManager.instance.path_b;
        Debug.Log("minCost1: " + minCost1 + "  " + "minCost2 :" + minCost2 + "  ");
        //"useless: " + uselessRedCounters.Count);
        if (GameManager.instance.pathChange == 0)
        {

            if (minCost1 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost1 && GameManager.instance.pathChange == 0)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
                // Collect all red pickups from generators
                GameManager.instance.path_current = pathA;

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
        }
        //ALL path a not avaliable, change to path B
        else if (GameManager.instance.pathChange == 1)
        {
            if (minCost2 <= GameParameters.instance.carryLimit * GameParameters.instance.shuttleNum && redPickups.Count >= minCost2)
            //+uselessRedCounters.Count >= minCost)
            {
                //pathA = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(closestAnchorsRed[0], closestAnchorsRed[1], true));
                //GameManager.instance.path_current = pathB;
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

        // in order choose true path
        //Vector3[] closestAnchorsRed2 = FindCheapestChain(closestAnchorsRed, true);
        int tryNum = Random.Range(0, 3);
        // Choose fake path
        Vector3[] firstAnchors = { GameManager.instance.anchor_a1, GameManager.instance.anchor_a2 };
        Vector3[] secondAnchors = { GameManager.instance.anchor_a3, GameManager.instance.anchor_a4 };
        //Vector3[] fakeAnchors = FindCheapestChain(trueAnchors, false);
        Debug.Log("first anchors: " + firstAnchors[0] + "  " + firstAnchors[1]);

        GameManager.instance.trueAnchorPos[0] = firstAnchors[0];
        GameManager.instance.trueAnchorPos[1] = firstAnchors[1];
        Debug.Log("second anchors: " + secondAnchors[0] + "  " + secondAnchors[1]);

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

        // in order deposit
        Vector3 pos;
        //List<int> randomOrder = RandomOrder(actions.GetPickupColor().Sum());
        List<int> inOrder = InOrder(actions.GetPickupColor().Sum());
        foreach (int i in inOrder)
        {
            Debug.Log("index: " + i + "   " + "color: " + bag[i]);
            //tryNum = Random.Range(0, 3);
            List<Vector3> positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2), true));

            //change according to the pathChange para
            if (GameManager.instance.pathChange == 0)
            {
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //positions = GameManager.instance.path_current;
                    GameManager.instance.path_current = positions;


                    //Vector3 pos;
                    //if no path a(out of the narrative region),change to path b

                    //pos = Methods.instance.InorderPosition(positions);
                    Debug.Log(" the posistion is " + positions.Count);
                    //!!!!! current nothing



                    if (Methods.instance.Contains(Methods.instance.non_contiguous(positions, turn,minCost1, GameManager.instance.depositRed), firstAnchors) == false ||positions.Count==0)
                    {
                        
                        GameManager.instance.pathChange++;
                        positions = GameManager.instance.path_b;
                        GameManager.instance.path_current = GameManager.instance.path_b;
                        //??????
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        //pos = Methods.instance.non_contiguous(positions, turn, GameManager.instance.depositRed);
                        Debug.Log("Deposit path b cause no path a: " + pos);

                    }
                    else
                    {
                        pos = Methods.instance.non_contiguous(positions, turn,minCost1, GameManager.instance.depositRed);
                        
                        GameManager.instance.Total_RealPath_Blocks++;
                        Debug.Log("Deposit real: " + pos);
                    }
                    //if (GameManager.instance.firstBlock == false)
                    //{
                    //    GameManager.instance.Unblocked_Reds++;
                    //    Debug.Log("Deposit red before the first block:" + GameManager.instance.Unblocked_Reds);
                    //}
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }

                else if (bag[i] != 0)
                {

                    pos = Vector3.zero;

                    List<Vector3> a1_a2_narrative = Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2));
                    List<Vector3> tiles_for_nonRed=new List<Vector3>();
                    tiles_for_nonRed.Clear();
                    //pos = Methods.instance.RandomPosition(Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2)));
                    foreach (Vector3 narrative_tile in a1_a2_narrative)
                    {
                        if (Methods.instance.IsEmptyGrid(narrative_tile) )
                        {
                            tiles_for_nonRed.Add(narrative_tile);
                        }

                    }
                    if (tiles_for_nonRed.Count == 0)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                    }
                    else
                    {
                        pos = Methods.instance.RandomPosition(tiles_for_nonRed);
                    }
                    //pos = Methods.instance.RandomPosition(Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2)));


                    Debug.Log("Deposit Elsewhere: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }
            else if (GameManager.instance.pathChange == 1)
            {
                positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a3), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a4), true));
                GameManager.instance.path_current = positions;
                if (bag[i] == 0)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //Vector3 pos;
                    //if no path a(out of the narrative region), stop change
                    if (Methods.instance.Contains(Methods.instance.non_contiguous(positions, turn,minCost2, GameManager.instance.depositRed), secondAnchors) == false ||positions.Count==0)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        turn = 0;
                    }
                    else
                    {
                        pos = Methods.instance.non_contiguous(positions, turn,minCost2, GameManager.instance.depositRed);

                        //GameManager.instance.Total_RealPath_Blocks++;
                        Debug.Log("Deposit real: " + pos);
                    }

                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }

                else if (bag[i] != 0)
                {

                    pos = Vector3.zero;

                    List<Vector3> a1_a2_narrative = Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2));
                    List<Vector3> tiles_for_nonRed = new List<Vector3>();
                    tiles_for_nonRed.Clear();
                    //pos = Methods.instance.RandomPosition(Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2)));
                    foreach (Vector3 narrative_tile in a1_a2_narrative)
                    {
                        if (Methods.instance.IsEmptyGrid(narrative_tile))
                        {
                            tiles_for_nonRed.Add(narrative_tile);
                        }

                    }
                    if (tiles_for_nonRed.Count == 0)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                    }
                    else
                    {
                        pos = Methods.instance.RandomPosition(tiles_for_nonRed);
                    }
                    //pos = Methods.instance.RandomPosition(Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2)));


                    Debug.Log("Deposit Elsewhere: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }


            bag[i] = -1;

        }

        return actions;
    }

    //????
    public Actions MakeDecisionSeven(List<Actions> AIactions, int turn)

    {
        Actions actions = new Actions();
        Debug.Log("now is turn :" + turn);

       Vector3[] closestAnchorsRed = FindCheapestChain(new Vector3[2], true);
        
        // Check if can win the game this turn
        //List<Vector3> uselessRedCounters = GetUselessRedCounters(closestAnchorsRed);
        List<Vector3> redPickups = GetAllRedPickups();
        int minCost1 = FindChainCost(GameManager.instance.anchor_a1, GameManager.instance.anchor_a2, true);
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
       
        foreach (int i in randomOrder)
        {
            Debug.Log("index: " + i + "   " + "color: " + bag[i]);
            //imete as coin
            tryNum = Random.Range(0, 3);
            List<Vector3> positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2), true));
            //even
            Debug.Log("the coin is " + tryNum);
           


            if (GameManager.instance.pathChange == 0)
            {
                if (bag[i] == 0 && tryNum % 2 == 1)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //positions = GameManager.instance.path_current;
                    GameManager.instance.path_current = positions;

                    //Vector3 pos;
                    //if no path a(out of the narrative region),change to path b

                    //pos = Methods.instance.InorderPosition(positions);
                    Debug.Log(" the posistion is " + positions.Count);
                    //!!!!! current nothing



                    if (Methods.instance.Contains(Methods.instance.non_contiguous(positions, turn, minCost1, GameManager.instance.depositRed), firstAnchors) == false)
                    {
                        //pos = GetRandomEmptyGrid(AIactions, actions);
                        //Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        positions = GameManager.instance.path_b;
                        GameManager.instance.path_current = GameManager.instance.path_b;
                        //??????
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        //pos = Methods.instance.non_contiguous(positions, turn,minCost2, GameManager.instance.depositRed);
                        Debug.Log("Deposit path b cause no path a: " + pos);
                        //GameManager.instance.Real_Path_Replan = turn + 1;
                        //GameManager.instance.gameLog += "the replaned time" + GameManager.instance.Real_Path_Replan + "\n";


                    }
                    else
                    {
                        pos = Methods.instance.non_contiguous(positions, turn, minCost1, GameManager.instance.depositRed);

                        Debug.Log("Deposit real: " + pos);
                    }
                    
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }
                else if (bag[i] == 0 && tryNum % 2 == 0)
                {
                    List<Vector3> a1_a2_narrative = Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2));
                    List<Vector3> a3_a4_narrative= Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a3), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a4));
                    List<Vector3> tiles_for_Red = new List<Vector3>();
                    tiles_for_Red.Clear();
                    //pos = Methods.instance.RandomPosition(Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2)));
                    foreach (Vector3 narrative_tile in a1_a2_narrative)
                    {
                        if (Methods.instance.IsEmptyGrid(narrative_tile) && a3_a4_narrative.Contains(narrative_tile))
                        {
                            tiles_for_Red.Add(narrative_tile);
                        }

                    }
                    if (tiles_for_Red.Count == 0)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                    }
                    else
                    {
                        pos = Methods.instance.RandomPosition(tiles_for_Red);
                    }
                    //pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit red Elsewhere: " + pos);
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));
                }

                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    List<Vector3> a1_a2_narrative = Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2));
                    List<Vector3> tiles_for_nonRed = new List<Vector3>();
                    tiles_for_nonRed.Clear();
                    //pos = Methods.instance.RandomPosition(Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2)));
                    foreach (Vector3 narrative_tile in a1_a2_narrative)
                    {
                        if (Methods.instance.IsEmptyGrid(narrative_tile))
                        {
                            tiles_for_nonRed.Add(narrative_tile);
                        }

                    }
                    if (tiles_for_nonRed.Count == 0)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                    }
                    else
                    {
                        pos = Methods.instance.RandomPosition(tiles_for_nonRed);
                    }
                    //pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }
            else if (GameManager.instance.pathChange == 1)
            {
                positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a3), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a4), true));
                GameManager.instance.path_current = positions;
                if (bag[i] == 0 && tryNum % 2 == 1)
                {
                    //List<Vector3> positions = GameManager.instance.path_a;
                    positions = RemovePositionsFromList(positions, actions.GetDepositPos(AIactions));
                    //Vector3 pos;
                    //if no path a(out of the narrative region), stop change
                    if (Methods.instance.Contains(Methods.instance.non_contiguous(positions, turn, minCost2, GameManager.instance.depositRed), SecondAnchors) == false)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                        Debug.Log("Deposit Elsewhere cause no path: " + pos);
                        //GameManager.instance.Total_FalsePath_Blocks++;
                        GameManager.instance.pathChange++;
                        //turn = 0;
                    }
                    else
                    {
                        pos = Methods.instance.non_contiguous(positions, turn, minCost2, GameManager.instance.depositRed);

                        //GameManager.instance.Total_RealPath_Blocks++;
                        Debug.Log("Deposit real: " + pos);
                    }

                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));

                }
                else if (bag[i] == 0 && tryNum % 2 == 0)
                {
                    List<Vector3> a1_a2_narrative = Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2));
                    List<Vector3> a3_a4_narrative = Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a3), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a4));
                    List<Vector3> tiles_for_Red = new List<Vector3>();
                    tiles_for_Red.Clear();
                    //pos = Methods.instance.RandomPosition(Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2)));
                    foreach (Vector3 narrative_tile in a1_a2_narrative)
                    {
                        if (Methods.instance.IsEmptyGrid(narrative_tile) && a3_a4_narrative.Contains(narrative_tile))
                        {
                            tiles_for_Red.Add(narrative_tile);
                        }

                    }
                    if (tiles_for_Red.Count == 0)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                    }
                    else
                    {
                        pos = Methods.instance.RandomPosition(tiles_for_Red);
                    }
                    Debug.Log("Deposit red Elsewhere: " + pos);
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));
                }

                else if (bag[i] != 0)
                {
                    //Vector3 pos;
                    List<Vector3> a1_a2_narrative = Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2));
                    List<Vector3> tiles_for_nonRed = new List<Vector3>();
                    tiles_for_nonRed.Clear();
                    //pos = Methods.instance.RandomPosition(Methods.instance.TilesNarrativeRegion(Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a1), Methods.instance.TransAnchorPositionInGrid(GameManager.instance.anchor_a2)));
                    foreach (Vector3 narrative_tile in a1_a2_narrative)
                    {
                        if (Methods.instance.IsEmptyGrid(narrative_tile))
                        {
                            tiles_for_nonRed.Add(narrative_tile);
                        }

                    }
                    if (tiles_for_nonRed.Count == 0)
                    {
                        pos = GetRandomEmptyGrid(AIactions, actions);
                    }
                    else
                    {
                        pos = Methods.instance.RandomPosition(tiles_for_nonRed);
                    }
                    //pos = GetRandomEmptyGrid(AIactions, actions);
                    Debug.Log("Deposit Elsewhere: " + pos);
                    //GameManager.instance.Total_FalsePath_Blocks++;
                    actions.MoveTo(pos);
                    actions.DepositIndexAt(pos, i, Random.Range(0.1f, 2f));


                }

            }


            bag[i] = -1;

        }

        return actions;
    }
}

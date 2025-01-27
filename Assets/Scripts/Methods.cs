﻿/*
 * This class contains several general methods and algorithms that may help you develop your agent.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//using System;
//using System;

public class Methods : MonoBehaviour
{
    public static Methods instance;

    private List<List<Vector3>> brokenLine = new List<List<Vector3>>();
    private List<List<bool>> visited = new List<List<bool>>();
    private int[] dx = { -1, 1, 0, 0 };
    private int[] dy = { 0, 0, -1, 1 };

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Returns a random position from the list
    public Vector3 RandomPosition(List<Vector3> list)
    {
        int randomIndex = Random.Range(0, list.Count);
        return list[randomIndex];
    }
    public Vector3 InorderPosition(List<Vector3> list)
    {     int randomIndex = 0;
            return list[randomIndex]; 
    }

    // pos = Methods.instance.Random_InoderPosition(positions, turn, GameManager.instance.depositRed);
    //?????
    public Vector3 Random_InoderPosition(List<Vector3> list,int turn_times, List<Vector3> list2)
    {
        int randomIndex=0;
        if (turn_times == 0|list2.Count==0)
        {
            randomIndex = Random.Range(0, list.Count);
            //return list[randomIndex];
           // Debug.Log("the relevant random is"+randomIndex);
        }
        else
        {
            foreach (Vector3 pos in list)
            {
                if (list2.Contains(pos))
                {
                    randomIndex = list.IndexOf(pos) + 1;
                    break;
                }
            }
        }
        return list[randomIndex];
    }

    // pos = Methods.instance.Random_InoderPosition(positions, turn, GameManager.instance.depositRed);
    public Vector3 non_contiguous(List<Vector3> list, int turn_times,int min, List<Vector3> list2)
    {
        int randomIndex = 0;
        //int index = 1;
        int index1 = 0;
        if (turn_times == 0|list2.Count==0)
        {
            randomIndex = Random.Range(0, list.Count);
        }
        else
        {
            for (int index = 1; index < list.Count - 1;)
                
            {
                
                
                index1 = index;
                index++;
                Vector3 pos = list[index1];
                if (list2.Contains(list[list.IndexOf(pos)-1])==false && list2.Contains(list[list.IndexOf(pos) +1])==false && index1<(min/2))
                {
                    randomIndex = list.IndexOf(pos);
                    
                    break;
                }
                else {
                    randomIndex = Random.Range(0, list.Count);
                    break;
                }
               
            }
        }
        return list[randomIndex];
    }

    public Vector3 non_contiguous_nonRed(List<Vector3> list)
    {
        int randomIndex = 0;
        //int index = 1;
        int index1 = 0;
        
       
        
        
            for (int index = 1; index < list.Count - 1;)

            {
                index1 = index;
                index++;
                Vector3 pos = list[index1];
                if (IsEmptyGrid(list[list.IndexOf(pos) - 1]) && IsEmptyGrid(list[list.IndexOf(pos) + 1])&& index1 < (list.Count / 2))
                {
                    randomIndex = list.IndexOf(pos);

                    break;
                }
                else
                {
                    randomIndex = Random.Range(0, list.Count);
                    break;
                }

            
        }
        return list[randomIndex];
    }


    //If the pos is on one anchor, return the position of the anchor's center, else return Vector3.zero
    public Vector3 IsOnAnAnchor(Vector3 pos)
    {
        //float Xmin;
        //float Xmax;
        //float Ymin;
        //float Ymax;
        foreach (Vector3 position in GameManager.instance.anchorPositions)
        {
            //wrong !!!!!!!!!!
            if (Vector3.Distance(position, pos)< 1f)
            {
                return position;
            }
            
        }
        return Vector3.zero;
    }

    // Checks if the position is valid on the board
    public bool IsOnBoard(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < GameParameters.instance.gridSize && pos.y >= 0 && pos.y < GameParameters.instance.gridSize)
        {
            return true;
        }
        return false;
    }

    // Checks if the position is empty grid
    public bool IsEmptyGrid(Vector3 pos)
    {
        if (GameManager.instance.deposited[(int)pos.x][(int)pos.y] == -1 && IsOnAnAnchor(pos) == Vector3.zero && TileExist(pos))
        {
            return true;
        }
        return false;
    }

    // Checks if two positions are adjacent
    public bool IsAdjGrid(Vector3 pos1, Vector3 pos2)
    {
        if (Mathf.Approximately(Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y),1f))
        {
            return true;
        }
        return false;
    }

    
    

    //record all the tile in the region list，

    public List<Vector3> TilesNarrativeRegion(Vector3 anchor1,Vector3 anchor2)
    {
        List<Vector3> narrative_region = new List<Vector3>();
        float Xmin = anchor1.x;
        float Xmax = anchor1.x;
        float Ymin = anchor1.y;
        float Ymax = anchor1.y;
        Vector3[] m_aptVertices={anchor1, anchor2};
        Debug.Log("the narrative region from to"+ anchor1+anchor2);
        foreach (Vector3 pt in m_aptVertices)
        {
            if (Xmin > pt.x)
                Xmin = pt.x;

            if (Xmax < pt.x)
                Xmax = pt.x;

            if (Ymin > pt.y)
                Ymin = pt.y;

            if (Ymax < pt.y)
                Ymax = pt.y;
        }
        Xmin = Mathf.Floor(Xmin);
        Xmax = Mathf.Ceil(Xmax);
        Ymin = Mathf.Floor(Ymin);
        Ymax = Mathf.Ceil(Ymax);

        Debug.Log("check if all the region in the list"+Xmin+Xmax+Ymin+Ymax);
        for (int x= (int)Xmin; x<=Xmax;x++)
        {
            for (int y=(int)Ymin;y<=Ymax;y++)
            {
                if(IsOnAnAnchor(new Vector3(x, y, 0f))==Vector3.zero)
                {
                    narrative_region.Add(new Vector3(x, y, 0f));
                }
            }
            
        }
        Debug.Log(narrative_region.Count);
            
            return narrative_region;
    }





    public Vector3 FindAdjForTile(int x, int y)
    {
        Vector3 pos = Vector3.zero;
        List<Vector3> pos_adj= new List<Vector3>();
        pos_adj.Add(new Vector3(x + 1, y, 0f));
        pos_adj.Add(new Vector3(x - 1, y, 0f));
        pos_adj.Add(new Vector3(x, y - 1, 0f));
        pos_adj.Add(new Vector3(x, y + 1, 0f));
        while (pos==Vector3.zero)
        {
            int index = Random.Range(0, pos_adj.Count);
            if (IsEmptyGrid(pos_adj[index]) && IsOnBoard(pos_adj[index]) == true)
            {
                pos = pos_adj[index];
                break;
            }
            else
            {
                if (pos_adj.Count > 1) { 
                pos_adj.Remove(pos_adj[index]);
                 }
                else
                {
                    break;
                }
            }

        }

        return pos;
    }

    public Vector3 FindAdjTileForAnchor()
    {
        Vector3 pos = new Vector3(0, 0, 0);
        return pos;
    }



    // and  change it in the narrtive region
    public Vector3 FindAdjForAnchor(Vector3 pos_temp, List<Vector3> anchors_list)
    {

        Vector3 pos = Vector3.zero;
        float distance = Mathf.Infinity;
        Vector3 anotherAnchor = new Vector3(0, 0, 0);
        foreach (Vector3 anchor_pos in anchors_list)
        {
            if (anchor_pos != pos_temp && distance> Vector3.Distance(anchor_pos, pos_temp))
            {
                distance = Vector3.Distance(anchor_pos,pos_temp);
                anotherAnchor = anchor_pos;
            }
        }
        List<Vector3> narrative_region = TilesNarrativeRegion(pos_temp,anotherAnchor);
        distance = Mathf.Infinity;
        //find the near block position in narrative region
        foreach (Vector3 tile in narrative_region)
        {
            if (IsEmptyGrid(tile) && distance>Vector3.Distance(tile,pos_temp))
            {
                pos = tile;
                distance = Vector3.Distance(tile, pos_temp);
            }
           
        }
        //try
        //{
        //    if (IsEmptyGrid(new Vector3(pos_temp.x-0.5f,pos_temp.y-1.5f, 0f)) && IsOnBoard(new Vector3(pos_temp.x - 0.5f, pos_temp.y - 1.5f, 0f)) == true )
        //    {
        //        pos = new Vector3(pos_temp.x - 0.5f, pos_temp.y - 1.5f, 0f);
        //    }
        //    else if (IsEmptyGrid(new Vector3(pos_temp.x + 0.5f, pos_temp.y - 1.5f, 0f)) && IsOnBoard(new Vector3(pos_temp.x + 0.5f, pos_temp.y - 1.5f, 0f)) == true)
        //    {
        //        pos = new Vector3(pos_temp.x + 0.5f, pos_temp.y - 1.5f, 0f);
        //    }
        //    else if (IsEmptyGrid(new Vector3(pos_temp.x + 1.5f, pos_temp.y - 0.5f, 0f)) && IsOnBoard(new Vector3(pos_temp.x + 1.5f, pos_temp.y - 0.5f, 0f)) == true)
        //    {
        //        pos = new Vector3(pos_temp.x + 1.5f, pos_temp.y - 0.5f, 0f);
        //    }
        //    else if (IsEmptyGrid(new Vector3(pos_temp.x + 1.5f, pos_temp.y + 0.5f, 0f)) && IsOnBoard(new Vector3(pos_temp.x + 1.5f, pos_temp.y + 0.5f, 0f)) == true)
        //    {
        //        pos = new Vector3(pos_temp.x + 1.5f, pos_temp.y + 0.5f, 0f);
        //    }
        //    else if (IsEmptyGrid(new Vector3(pos_temp.x + 0.5f, pos_temp.y + 1.5f, 0f)) && IsOnBoard(new Vector3(pos_temp.x + 0.5f, pos_temp.y + 1.5f, 0f)) == true)
        //    {
        //        pos = new Vector3(pos_temp.x + 0.5f, pos_temp.y + 1.5f, 0f);
        //    }
        //    else if (IsEmptyGrid(new Vector3(pos_temp.x - 0.5f, pos_temp.y + 1.5f, 0f)) && IsOnBoard(new Vector3(pos_temp.x - 0.5f, pos_temp.y + 1.5f, 0f)) == true)
        //    {
        //        pos = new Vector3(pos_temp.x - 0.5f, pos_temp.y + 1.5f, 0f);
        //    }
        //    else if (IsEmptyGrid(new Vector3(pos_temp.x - 1.5f, pos_temp.y + 0.5f, 0f)) && IsOnBoard(new Vector3(pos_temp.x - 1.5f, pos_temp.y + 0.5f, 0f)) == true)
        //    {
        //        pos = new Vector3(pos_temp.x - 1.5f, pos_temp.y + 0.5f, 0f);
        //    }

        //    else if (IsEmptyGrid(new Vector3(pos_temp.x - 1.5f, pos_temp.y - 0.5f, 0f)) && IsOnBoard(new Vector3(pos_temp.x - 1.5f, pos_temp.y - 0.5f, 0f)) == true)
        //    {
        //        pos = new Vector3(pos_temp.x - 1.5f, pos_temp.y - 0.5f, 0f);
        //    }
            
        //}

        //catch (KeyNotFoundException)
        //{
        //    pos = new Vector3(0, 0, 0);
        //}

        return pos;
    }
    // Returns the generator ID the parking position belongs
    public int FindGenerator(Vector3 pos)
    {
        for (int i = 0; i < GameManager.instance.generators.Count; i++)
        {
            if (GameManager.instance.parkingPos[i] == pos)
            {
                return i;
            }
        }
        return -1;
    }

    // Checks if the position belongs to the generator
    public bool IsInGn(Vector3 pos, int generatorId)
    {
        GameObject generator = GameManager.instance.generators[generatorId];
        foreach (GameObject pickups in generator.GetComponent<GeneratorManager>().GetPickupsInGn())
        {
            if (pickups.transform.position == pos)
            {
                return true;
            }
        }
        return false;
    }

    // Returns the generator ID if the pos has a pickup
    public int OnPickup(Vector3 pos)
    {
        for (int i = 0; i < GameManager.instance.generators.Count; i++)
        {
            List<GameObject> pickups = GameManager.instance.generators[i].GetComponent<GeneratorManager>().GetPickupsInGn();
            for (int j = 0; j < pickups.Count; j++)
            {
                if (pickups[j].transform.position == pos)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    // Returns counter's color at pos, if no counter, return -1
    public int OnCounter(Vector3 pos)
    {
        if (IsOnBoard(pos) && GameManager.instance.deposited[(int)pos.x][(int)pos.y] > -1)
        {
            return GameManager.instance.deposited[(int)pos.x][(int)pos.y];
        }
        return -1;
    }

    public bool ReadyToTurnOver(Vector3 pos)
    {
        if (IsOnBoard(pos))
        {
            return GameManager.instance.readyToTurnOver[(int)pos.x][(int)pos.y];
        }
        return false;
    }


    public void SetReadyToTurnOver(Vector3 pos, bool ready)
    {
        if (IsOnBoard(pos))
        {
            GameManager.instance.readyToTurnOver[(int)pos.x][(int)pos.y] = ready;
        }
    }

    List<GameObject> waitToDestoryCounter = new List<GameObject>();
    List<GameObject> waitToActiveCounter = new List<GameObject>();
    List<GameObject> shuttles = new List<GameObject>();

    public void InitializeMethods()
    {
        waitToDestoryCounter.Clear();
        waitToActiveCounter.Clear();
        shuttles.Clear();
    }

    public IEnumerator TurnWhiteCounterOver(Vector3 pos, float turnOverDelay, GameObject shuttle)
    {
        GameObject[] counters;
        counters = GameObject.FindGameObjectsWithTag("WhiteCounter");
        foreach (GameObject counter in counters)
        {
            if (counter != null && counter.transform.position == pos)
            {
                counter.SetActive(false);
                if (OnCounter(pos) == -1) continue;
                GameObject colorCounter = LayoutObject(GameManager.instance.counterTiles[OnCounter(pos)], pos.x, pos.y);
                yield return new WaitForSeconds(turnOverDelay);
                // Do not turn back if game over or keep collision with shuttle
                if (!GameManager.instance.gameOver && !colorCounter.GetComponent<BoxCollider2D>().IsTouching(shuttle.GetComponent<BoxCollider2D>()))
                {
                    Destroy(colorCounter);
                    counter.SetActive(true);
                }
                else
                {
                    waitToDestoryCounter.Add(colorCounter);
                    waitToActiveCounter.Add(counter);
                    shuttles.Add(shuttle);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (waitToDestoryCounter.Count == 0) return;
        if (!GameManager.instance.gameOver && !waitToDestoryCounter[0].GetComponent<BoxCollider2D>().IsTouching(shuttles[0].GetComponent<BoxCollider2D>()))
        {
            Destroy(waitToDestoryCounter[0]);
            if (GameManager.instance.deposited[(int)waitToActiveCounter[0].transform.position.x][(int)waitToActiveCounter[0].transform.position.y] == -1)
            {
                Destroy(waitToActiveCounter[0]);
            }
            else
            {
                waitToActiveCounter[0].SetActive(true);
            }
            waitToDestoryCounter.RemoveAt(0);
            waitToActiveCounter.RemoveAt(0);
            shuttles.RemoveAt(0);
        }
    }

    public void TurnAllWhiteCounterOver()
    {
        GameObject[] counters;
        counters = GameObject.FindGameObjectsWithTag("WhiteCounter");
        foreach (GameObject counter in counters)
        {
            if (OnCounter(counter.transform.position) != -1)
            {
                LayoutObject(GameManager.instance.counterTiles[OnCounter(counter.transform.position)], counter.transform.position.x, counter.transform.position.y);
            }
            Destroy(counter);
        }
    }

    // Checks if the position hasn't been blocked
    public bool TileExist(Vector3 pos)
    {
        return !GameManager.instance.blocked[(int)pos.x][(int)pos.y];
    }

    public void BlockTile(Vector3 pos)
    {
        GameManager.instance.blocked[(int)pos.x][(int)pos.y] = true;
    }

    // Randomly chooses from yellow and blue
    public int RandomCarryCounter(int[] carry)
    {
        List<int> bag = new List<int>();
        for (int i = 1; i < 3; i ++)
        {
            for (int j = 0; j < carry[i]; j++)
            {
                bag.Add(i);
            }
        }
        return bag[UnityEngine.Random.Range(0, bag.Count)];
    }

    // Removes all deposited grids and anchors in the given list
    public List<Vector3> RemoveDepositedAndAnchor(List<Vector3> list)
    {
        List<Vector3> validList = new List<Vector3>();
        for (int i = 0; i < list.Count; i++)
        {
            if (IsEmptyGrid(list[i]))
            {
                validList.Add(list[i]);
            }
        }
        return validList;
    }

    // Finds num neighbors around list, return empty list if it is hard to find
    public List<Vector3> FindEmptyNeighbor(List<Vector3> list, int num, List<Vector3> addedToDeposit, int neighborRange)
    {
        List<Vector3> neighbor = new List<Vector3>();
        int randomCount = 0;
        while (num > 0)
        {
            Vector3 pos = list[UnityEngine.Random.Range(0, list.Count)];
            Vector3 newNeighbor = new Vector3(pos.x + UnityEngine.Random.Range(0, neighborRange), pos.y + UnityEngine.Random.Range(0, neighborRange), 0f);
            if (IsEmptyGrid(newNeighbor) && !addedToDeposit.Contains(newNeighbor) && !list.Contains(newNeighbor) && !neighbor.Contains(newNeighbor))
            {
                neighbor.Add(newNeighbor);
                num--;
            }
            // Hard to find neighbor in range
            randomCount++;
            if (randomCount > 50)
            {
                return new List<Vector3>();
            }
        }
        return neighbor;
    }

    // Returns how many red, yellow and blue counters can be picked up if move according to list, carryLimit is n
    public int[] PickupColorInPos(List<Vector3> list, int n)
    {
        int[] carry = new int[3];
        foreach (Vector3 pos in list)
        {
            if (carry.Sum() == n) break;
            int id = OnPickup(pos);
            if (id > -1)
            {
                carry[GameManager.instance.generators[id].GetComponent<GeneratorManager>().GetPickupsInGnColor(pos)]++;
            }
        }
        return carry;
    }

    // Returns the generator which has the most number of red pickups, if no red counter in any generator, return the first generator
    public int MostRedGenerator()
    {
        int mostCount = -1;
        int bestGenerator = 0;
        for (int i = 0; i < GameManager.instance.generators.Count; i++)
        {
            int nowCount = GameManager.instance.generators[i].GetComponent<GeneratorManager>().GetRedPickupsNumber();
            if (nowCount > mostCount)
            {
                mostCount = nowCount;
                bestGenerator = i;
            }
        }
        return bestGenerator;
    }

    // Returns Anchor with the shortest linear distance except pos in list
    public Vector3 SearchClosestAnchor(List<Vector3> list)
    {
        Vector3 anchor = Vector3.zero;
        float dist = Mathf.Infinity;
        foreach (Vector3 position in GameManager.instance.anchorPositions)
        {
            if (Vector3.Distance(transform.position, position) < dist && !list.Contains(position))
            {
                dist = Vector3.Distance(transform.position, position);
                anchor = position;
            }
        }
        return anchor;
    }

    // Returns random Anchor except pos in list
    public Vector3 RandomAnchor(List<Vector3> list)
    {
        int index = Random.Range(0, GameManager.instance.anchorPositions.Count);
        while (list.Contains(GameManager.instance.anchorPositions[index]))
        {
            index =Random.Range(0, GameManager.instance.anchorPositions.Count);
        }
        return GameManager.instance.anchorPositions[index];
    }

    // Transfer anchor's center position to a valid grid
    public Vector3 TransAnchorPositionInGrid(Vector3 position)
    {

        return new Vector3(position.x - 0.5f, position.y - 0.5f, 0f);
    }

    // Returns num pickups' positions in the generator
    public List<Vector3> PickupsPosInGn(int generatorId, int num)
    {
        List<Vector3> pickupsPos = new List<Vector3>();
        List<GameObject> pickups = GameManager.instance.generators[generatorId].GetComponent<GeneratorManager>().GetPickupsInGn();
        for (int i = 0; i < pickups.Count; i++)
        {
            //if (generator.GetComponent<GeneratorManager>().GetPickupsInGnColor(pickups[i].transform.position) == 0)
            pickupsPos.Add(pickups[i].transform.position);
            num--;
            if (num == 0) break;
        }
        return pickupsPos;
    }

    public bool OnParkingPos(Vector3 pos)
    {
        for (int i = 0; i < GameManager.instance.generators.Count; i++)
        {
            if (GameManager.instance.parkingPos[i] == pos)
            {
                return true;
            }
        }
        return false;
    }

    // Returns a path in grid from start to end
    // if onlyRedCounter == true, the path only throughs red counters
    public List<Vector3> FindPathInGrid(Vector3 start, Vector3 end, bool onlyRedCounter)
    {
        InitialisePath();
        brokenLine[(int)start.x][(int)start.y] = new Vector3(-1, -1, 0f);   //Unvisited positions are -2
        Queue queue = new Queue();
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Vector3 now = (Vector3)queue.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = new Vector3(now.x + dx[i], now.y + dy[i], 0f);
                if (IsOnBoard(pos) && TileExist(pos) && brokenLine[(int)pos.x][(int)pos.y].x < -1)
                {
                    if (!onlyRedCounter || IsOnAnAnchor(pos) != Vector3.zero || GameManager.instance.deposited[(int)pos.x][(int)pos.y] == -1 || GameManager.instance.deposited[(int)pos.x][(int)pos.y] == 0)
                    {
                        queue.Enqueue(pos);
                        brokenLine[(int)pos.x][(int)pos.y] = now;
                        if (Mathf.Abs(pos.x - end.x) < 1f && Mathf.Abs(pos.y - end.y) < 1f)
                        {
                            return FindPath(pos);
                        }
                    }
                }
            }
        }
        return new List<Vector3>();
    }

    public bool BFStoAnotherAnchor(Vector3 start)
    {
        Vector3 startAnchorCenter = IsOnAnAnchor(start);
        InitialiseVisited();
        Queue queue = new Queue();
        queue.Enqueue(start);
        visited[(int)start.x][(int)start.y] = true;
        while (queue.Count > 0)
        {
            Vector3 now = (Vector3)queue.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = new Vector3(now.x + dx[i], now.y + dy[i], 0f);
                if (IsOnBoard(pos))
                {
                    if (!visited[(int)pos.x][(int)pos.y] && (GameManager.instance.deposited[(int)pos.x][(int)pos.y] == 0 || IsOnAnAnchor(pos) != Vector3.zero))
                    {
                        queue.Enqueue(pos);
                        visited[(int)pos.x][(int)pos.y] = true;
                        if (IsOnAnAnchor(pos) != Vector3.zero && IsOnAnAnchor(pos) != startAnchorCenter)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private void InitialisePath()
    {
        brokenLine.Clear();
        for (int x = 0; x < GameParameters.instance.gridSize; x++)
        {
            brokenLine.Add(new List<Vector3>());
            for (int y = 0; y < GameParameters.instance.gridSize; y++)
            {
                brokenLine[x].Add(new Vector3(-2, -2, 0f));
            }
        }
    }

    private void InitialiseVisited()
    {
        visited.Clear();
        for (int x = 0; x < GameParameters.instance.gridSize; x++)
        {
            visited.Add(new List<bool>());
            for (int y = 0; y < GameParameters.instance.gridSize; y++)
            {
                visited[x].Add(false);
            }
        }
    }

    private List<Vector3> FindPath(Vector3 pos)
    {
        List<Vector3> pathList = new List<Vector3>();
        pathList.Clear();
        while (brokenLine[(int)pos.x][(int)pos.y].x >= 0)
        {
            pathList.Add(pos);
            pos = brokenLine[(int)pos.x][(int)pos.y];
        }
        pathList.Reverse();
        return pathList;
    }

    public GameObject LayoutObject(GameObject prefab, float x, float y)
    {
        Vector3 position = new Vector3(x, y, 0f);
        return Instantiate(prefab, position, Quaternion.identity);
    }

    //measure deceptive under ai wins statement


    //dignose of the anchor
    public bool Contains(Vector3 p, Vector3[] m_aptVertices)
    {
        bool bContains = true; //obviously wrong at the moment :)
        float Xmin = m_aptVertices[0].x;
        float Xmax = m_aptVertices[0].x;
        float Ymin = m_aptVertices[0].y;
        float Ymax = m_aptVertices[0].y;

        foreach (Vector3 pt in m_aptVertices)
        {
            if (Xmin > pt.x)
                Xmin = pt.x;

            if (Xmax < pt.x)
                Xmax = pt.x;

            if (Ymin > pt.y)
                Ymin = pt.y;

            if (Ymax < pt.y)
                Ymax = pt.y;
        }
        Xmin = Mathf.Floor(Xmin);
        Xmax = Mathf.Ceil(Xmax);
        Ymin = Mathf.Floor(Ymin);
        Ymax = Mathf.Ceil(Ymax) ;

       // Debug.Log("check if all the region in the list:" + Xmin+"," + Xmax + "," + Ymin + "," + Ymax);
        if (p.x < Xmin || p.x > Xmax || p.y < Ymin || p.y > Ymax)
            bContains = false;
        else
        {
            //figure out if the point is in the polygon
        }

        return bContains;
    }

    public float Ratio(int x, int y)
    {
        return (float)x/y ;
    }

    //Find shortest path in the grid betweent two anchor
    public int FindChainCost(Vector3 start, Vector3 end, bool onlyRed)
    {
        start = TransAnchorPositionInGrid(start);
        end = TransAnchorPositionInGrid(end);
        List<Vector3> emptyPos = RemoveDepositedAndAnchor(FindPathInGrid(start, end, onlyRed));
        //Debug.Log("start in :"+ start +" end:"+ end);
        //foreach (Vector3 pos in emptyPos)
        //{
        //    Debug.Log(pos);
        //}
        return emptyPos.Count;
    }
    public List<Vector3> FindChainPosition(Vector3 start, Vector3 end, bool onlyRed)
    {
        start = TransAnchorPositionInGrid(start);
        end = TransAnchorPositionInGrid(end);
        List<Vector3> emptyPos = RemoveDepositedAndAnchor(FindPathInGrid(start, end, onlyRed));
        //Debug.Log("start in :"+ start +" end:"+ end);
        //foreach (Vector3 pos in emptyPos)
        //{
        //    Debug.Log(pos);
        //}
        return emptyPos;
    }

    public int CalculateOverlap(Vector3 a1, Vector3 a2, Vector3 a3, Vector3 a4)
    {
        int overlap = 0;
        
        List<Vector3> tiles1=TilesNarrativeRegion(a1, a2);
        List<Vector3> tiles2 = TilesNarrativeRegion(a3, a4);
        
        foreach (Vector3 tile1 in tiles1)
        {
            if (tiles2.Contains(tile1))
            {
                overlap++;
            }
        }

        return overlap;

    }



    //task 1 set up
    public void Task1Anchor(List<Vector3> anchor_list, out int cost_a, out int cost_b)
    {
        Vector3 a1 = anchor_list[0];
        Vector3 a2= new Vector3();
        Vector3 a3= new Vector3();
        Vector3 a4 = new Vector3();
        cost_a = int.MaxValue;
        cost_b = int.MaxValue;
        foreach (Vector3 position in anchor_list)
        {
            if((position != a1)&&(FindChainCost(a1, position,true)<cost_a) )
            {
                cost_a = FindChainCost(a1, position, true);
                a2 = position;
            }
        }
        foreach (Vector3 position in anchor_list)
        {
            if ((position!=a1)&&(position!= a2))
            {
                a3 = position;
                break;
            }
        }
        foreach (Vector3 position in anchor_list)
        {
            if ((position != a3) && (FindChainCost(a3, position, true) < cost_b))
            {
                cost_b = FindChainCost(a3, position, true);
                a4 = position;
            }
        }
        GameManager.instance.anchor_a1 = a1;
        //Debug.Log("after tanrsfer to the anchor is"+TransAnchorPositionInGrid(a1));
        GameManager.instance.anchor_a2 = a2;
        GameManager.instance.anchor_a3 = a3;
        GameManager.instance.anchor_a4 = a4;
        GameManager.instance.path_a = FindChainPosition(a1, a2, true);
        GameManager.instance.path_b = FindChainPosition(a3, a4, true);
        Debug.Log("a1" + a1);
        Debug.Log("a2" + a2);
        Debug.Log("a3" + a3);
        Debug.Log("a4" + a4);
    }



    public void Task2Set()
  
    {
        //GameManager.instance.anchor_a1 = new Vector3(0.5f, 7.5f, 0f);
        //GameManager.instance.anchor_a2 = new Vector3(7.5f, 7.5f, 0f);
        //GameManager.instance.anchor_a3 = new Vector3(0.5f, 0.5f, 0f);

        //GameManager.instance.anchor_a4 = new Vector3(7.5f, 13.5f, 0f);
        GameManager.instance.anchor_a1 = new Vector3(0.5f, 7.5f, 0f);
        GameManager.instance.anchor_a2 = new Vector3(12.5f, 7.5f, 0f);
        GameManager.instance.anchor_a3 = new Vector3(0.5F, 4.5F, 0f);
        GameManager.instance.anchor_a4 = new Vector3(7.5f, 12.5f, 0f);
        
        GameManager.instance.path_a = FindChainPosition(GameManager.instance.anchor_a1, GameManager.instance.anchor_a2, true);
        GameManager.instance.path_b = FindChainPosition(GameManager.instance.anchor_a3, GameManager.instance.anchor_a4, true);

        GameManager.instance.Task1_a= FindChainCost(GameManager.instance.anchor_a1, GameManager.instance.anchor_a2, true); 

        GameManager.instance.Task1_b = FindChainCost(GameManager.instance.anchor_a3, GameManager.instance.anchor_a4, true);

    }

    public bool IsPathAvailable(List<Vector3> path_positions, List<Vector3> block_pos,int changeTimes)
    {
        bool j= false;
        if (changeTimes!=1) {
            foreach (Vector3 blocks in block_pos)
            {
                if (path_positions.Contains(blocks) == true)
                {
                    j = true;
                    break;

                }
            }
        }
        return j;
    }
    public bool IsPathBloked(List<Vector3> path_positions, Vector3 pos)
    {
        bool flag = false;
        if (path_positions.Contains(pos))
        {
            flag = true;
        }
        return flag;
    }


    public bool IsBrokenLine(List<Vector3> path)
    {
        bool flag = false;
        Vector3 start = path[0];
        Vector3 end = path[path.Count-1];
        float slope=(end.y-start.y)/(end.x-start.x);
        float slope_temp;
        for (int j=0;j<path.Count-1;j++)
        {
            slope_temp = (path[j+1].y-path[j].y) / (path[j + 1].x - path[j].x);
            if (slope_temp.Equals(slope)!=true)
            {
                flag = true;
            }
        }
        
       
        return flag;
    }

    public float PointAndLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        float result= Mathf.Abs((lineEnd.x - lineStart.x) * (lineStart.y - point.y) - (lineStart.x - point.x) * (lineEnd.y - lineStart.y)) /
                Mathf.Sqrt(Mathf.Pow(lineEnd.x - lineStart.x, 2) + Mathf.Pow(lineEnd.y - lineStart.y, 2));
        return result;
    }
    public float PointAndBrokenLine(Vector3 point, List<Vector3> brokenLine)
    {
        List<float> results=new List<float>();
        Vector3 brokenPoint=brokenLine[0];
        Vector3 start = brokenLine[0];
        Vector3 end = brokenLine[1];
        float slope = (end.y - start.y) / (end.x - start.x);
        float slope_temp;
        for (int j = 0; j < brokenLine.Count - 1; j++)
        {
            slope_temp = (brokenLine[j + 1].y - brokenLine[j].y) / (brokenLine[j + 1].x - brokenLine[j].x);
          
            if (slope_temp.Equals(slope) != true)
            {
                slope = slope_temp;
                results.Add(PointAndLine(point, brokenPoint, brokenLine[j]));
                brokenPoint = brokenLine[j];
            }
        }
        return results.Min();
    }

}

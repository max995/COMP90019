using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HumanOne : MonoBehaviour
{
    // Start is called before the first frame update
    //check the game parameter in 
    public BoardGenerator bg;
    private Vector3 pos;
    private TileController tl;
    private UIManager uI;

    public Vector3 LastestRed(List<Vector3> redList)
    {

        if (redList.Count >= 1)
        {
            Vector3 lastRed = redList[redList.Count-1];
            pos = Methods.instance.FindAdjForTile((int)lastRed.x, (int)lastRed.y);
            Debug.Log("the pos is"+pos);
            if (pos != Vector3.zero)
            {
                return pos;
            }
            else
            {
                return Methods.instance.RandomPosition(bg.gridPositions);
            }
        }
        else
        {
            return Methods.instance.RandomPosition(bg.gridPositions);
        }
    }


    //public Vector3 LastestRed(string red_token)
    //{
    //    Vector3 pos = new Vector3();
    //    string[] red_tokens = red_token.Split('#');
    //    // Debug.Log(red_token);
    //    if (red_tokens.Length>1) {
            
    //        string lastRed = red_tokens[red_tokens.Length - 2];
    //        Debug.Log(red_tokens[red_tokens.Length - 2]);
    //        string[] posTemp = lastRed.Split('-');
    //        //Debug.Log(posTemp);
    //        int x = StringToInt(posTemp[0]);
    //        //Debug.Log(x);
    //        int y = StringToInt(posTemp[1]);
    //        //Debug.Log(y);
    //        pos = Methods.instance.FindAdjForTile(x, y);
    //        if (pos!=Vector3.zero)
    //        {
    //            return pos;
    //        }
    //        else
    //        {
    //            return Methods.instance.RandomPosition(bg.gridPositions);
    //        }
            
    //    }
    //    else

    //    {
    //        Debug.Log("I am the random but in ");
    //        return Methods.instance.RandomPosition(bg.gridPositions);
    //    }
    //}

    private int StringToInt(string s)
    {
        int num = -1;
        try
        {
            num = Int32.Parse(s);
            return num;
        }
        catch (FormatException)
        {
            return -1;
        }
    }
    
    private Vector3 HumanDecision(int humanHint,string reds)
    {
        Vector3 pos_human = new Vector3();
        if (humanHint == 1)
        {
            pos_human= Methods.instance.RandomPosition(bg.gridPositions);
        }
        if (humanHint==0)
        {
            Debug.Log("This is lastest red");
            pos_human = LastestRed(GameManager.instance.depositRed);
            //make red log clear each turn?
           // GameManager.instance.redToken = "";
        }
        if (humanHint == 2)
        {
            pos_human = RedNearAnchor(GameManager.instance.depositRed);
            //keep it in the path
            Debug.Log("I am the red near anchor"+pos_human);
        }
        if (humanHint==3)
        {
            pos_human = DensestRed(GameManager.instance.depositRed);
            Debug.Log("I am densest");
        }
        if (Methods.instance.IsEmptyGrid(pos_human) != true)
        {
            Debug.Log("I am the random but in ");
            pos_human = Methods.instance.RandomPosition(bg.gridPositions);
        }
        return pos_human;
    }

    private Vector3 RedNearAnchor(string red_token)
    {
        Vector3 shortDis = new Vector3();
        string[] red_tokens = red_token.Split('#');
        string lastRed = red_tokens[red_tokens.Length - 2];
        Debug.Log(red_tokens[red_tokens.Length - 2]);
        string[] posTemp = lastRed.Split('-');
        //Debug.Log(posTemp);
        float x = StringToFloat(posTemp[0]);
        float y = StringToFloat(posTemp[1]);
        Vector3 lastRedToken = new Vector3(x, y, 0f);
        float dist= Mathf.Infinity;
        foreach (Vector3 position in GameManager.instance.anchorPositions)
        {
            if (dist> Vector3.Distance(position, lastRedToken)) {
                dist = Vector3.Distance(position, lastRedToken);
                shortDis = position;
            }
        }
        Debug.Log("shortdis" + shortDis);
        shortDis = Methods.instance.FindAdjForAnchor(shortDis, GameManager.instance.anchorPositions);
        Debug.Log("shortdis" + shortDis);
        return shortDis;
    }

    //the more useful function
    private Vector3 RedNearAnchor(List<Vector3> redList)
    {
        Vector3 shortDis = new Vector3();
        float dist = Mathf.Infinity;
        if (redList.Count >= 1)
        {
            Vector3 lastRed = redList[redList.Count - 1];
            foreach (Vector3 position in GameManager.instance.anchorPositions)
            {
                if (dist > Vector3.Distance(position, lastRed))
                {
                    dist = Vector3.Distance(position, lastRed);
                    shortDis = position;
                }
            }
            shortDis = Methods.instance.FindAdjForAnchor(shortDis, GameManager.instance.anchorPositions);
            return shortDis;
        }
        else
        {
            return Methods.instance.RandomPosition(bg.gridPositions);
        }
        
       
    }



    private float StringToFloat(string s)
    {
        float num = -1;
        try
        {
            num = float.Parse(s);
            return num;
        }
        catch (FormatException)
        {
            return -1;
        }
    }

    private Vector3 DensestRed(List<Vector3> list_red)
    {
        if (list_red.Count >= 3)
        {
            int[] adjCounter = new int[list_red.Count - 1];
            for (int l = 0; l < list_red.Count - 1; l++)
            {
                for (int j = l + 1; j < list_red.Count - 1; j++)
                {

                    if (Vector3.Distance(list_red[l], list_red[j]) <= GameParameters.instance.minAnchorDis)
                    {

                        adjCounter[l]++;
                        adjCounter[j]++;

                    }
                }
            }
            int maxAdj = adjCounter.Max();
            int p = Array.IndexOf(adjCounter, maxAdj);
            pos = Methods.instance.FindAdjForTile((int)list_red[p].x, (int)list_red[p].y);
            if (pos != Vector3.zero)
            {
                return pos;
            }
            else
            {
                return LastestRed(list_red);
            }

        }
        else
        {
            return LastestRed(list_red);
        }
    }



  





    private void FixedUpdate()
    {
        //Debug.Log("try to check the");
        if (!GameManager.instance.gameOver && GameManager.instance.playerTurn && GameManager.instance.pathChange!=2)
        {
            Debug.Log("is true?" + GameManager.instance.playerTurn);
            //Debug.Log(GameManager.instance.redToken);
            string[] red_tokens = GameManager.instance.redToken.Split('#');
            //Debug.Log("red is :"+(red_tokens.Length - 1));
            uI = GetComponent<UIManager>();
            StartCoroutine(uI.ShowPlayerTurn());
            bg = GetComponent<BoardGenerator>();
            tl = GetComponent<TileController>();
            pos=HumanDecision(GameParameters.instance.humanType,GameManager.instance.redToken);
            GameObject blockTile = bg.tilePos[(int)pos.x, (int)pos.y];
            blockTile.GetComponent<Transform>().position = pos;
            while (Methods.instance.IsEmptyGrid(blockTile.transform.position))
            {
                GameManager.instance.blockedTile.Add(blockTile.transform.position);
                bg.gridPositions.Remove(pos);
                tl.AutoGrey(blockTile);
            }
            //Debug.Log("the current count"+ GameManager.instance.path_current.Count);
            //Debug.Log("the current start" + GameManager.instance.path_current[-1]);
            if (Methods.instance.IsPathBloked(GameManager.instance.path_current, blockTile.transform.position))
            {
                
                GameManager.instance.gameLog += "The block is on the real path \n";
                GameManager.instance.Total_RealPath_Blocks++;
                GameManager.instance.Total_RealPath_Blocks_Narrative++;
                GameManager.instance.Real_Path_Replan++;
            }
            else
            {

                if (GameManager.instance.pathChange == 0)//real path==a
                {
                    if (Methods.instance.IsBrokenLine(GameManager.instance.path_current) == false)
                    {
                        GameManager.instance.gameLog += "The distance between real path and block is :" +
                            Methods.instance.PointAndLine(blockTile.transform.position,
                            GameManager.instance.path_current[0],
                            GameManager.instance.path_current[GameManager.instance.path_current.Count - 1]) + "\n";

                        GameManager.instance.gameLog += "The distance between fake path and block is :" +
                            Methods.instance.PointAndLine(blockTile.transform.position,

                            GameManager.instance.path_b[0],
                            GameManager.instance.path_b[GameManager.instance.path_b.Count - 1]) + "\n";

                        if (Methods.instance.Contains(blockTile.transform.position, new Vector3[] { GameManager.instance.anchor_a1, GameManager.instance.anchor_a2 }))
                        {
                            GameManager.instance.Total_RealPath_Blocks_Narrative++;
                        }
                        else
                        {
                            GameManager.instance.Total_FalsePath_Blocks++;
                        }

                    }
                    else
                    {
                        GameManager.instance.gameLog += "The distance between real path and block is :" +
                            Methods.instance.PointAndBrokenLine(blockTile.transform.position, GameManager.instance.path_current)
                            + "\n";

                        GameManager.instance.gameLog += "The distance between fake path and block is :" +
                            Methods.instance.PointAndLine(blockTile.transform.position,
                            GameManager.instance.path_b[0],
                            GameManager.instance.path_b[GameManager.instance.path_b.Count - 1]) + "\n";
                        if (Methods.instance.Contains(blockTile.transform.position, new Vector3[] { GameManager.instance.anchor_a1, GameManager.instance.anchor_a2 }))
                        {
                            GameManager.instance.Total_RealPath_Blocks_Narrative++;
                        }
                        else
                        {
                            GameManager.instance.Total_FalsePath_Blocks++;
                        }
                    }
                    
                }
                else
                {
                    if (Methods.instance.IsBrokenLine(GameManager.instance.path_current) == false)
                    {
                        GameManager.instance.gameLog += "The distance between real path and block is :" +
                            Methods.instance.PointAndLine(blockTile.transform.position,
                            GameManager.instance.path_current[0],
                            GameManager.instance.path_current[GameManager.instance.path_current.Count - 1]) + "\n";
                        if (Methods.instance.Contains(blockTile.transform.position, new Vector3[] { GameManager.instance.anchor_a3, GameManager.instance.anchor_a4 }))
                        {
                            GameManager.instance.Total_RealPath_Blocks_Narrative++;
                        }
                        
                    }
                    else
                    {
                        GameManager.instance.gameLog += "The distance between real path and block is :" +
                            Methods.instance.PointAndBrokenLine(blockTile.transform.position, GameManager.instance.path_current)
                            + "\n";
                        if (Methods.instance.Contains(blockTile.transform.position, new Vector3[] { GameManager.instance.anchor_a3, GameManager.instance.anchor_a4 }))
                        {
                            GameManager.instance.Total_RealPath_Blocks_Narrative++;
                        }
                       

                    }
                    

                }
            }

        }

    }
}

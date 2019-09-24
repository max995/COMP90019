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

    public Vector3 LastestRed(string red_token)
    {
        Vector3 pos = new Vector3();
        string[] red_tokens = red_token.Split('#');
        // Debug.Log(red_token);
        if (red_tokens.Length>1) {
            
            string lastRed = red_tokens[red_tokens.Length - 2];
            Debug.Log(red_tokens[red_tokens.Length - 2]);
            string[] posTemp = lastRed.Split('-');
            //Debug.Log(posTemp);
            int x = StringToInt(posTemp[0]);
            //Debug.Log(x);
            int y = StringToInt(posTemp[1]);
            //Debug.Log(y);
            pos = Methods.instance.FindAdjForTile(x, y);
            return pos;
        }
        else
        {
            return Methods.instance.RandomPosition(bg.gridPositions);
        }
    }

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
        if (humanHint == 0)
        {
            pos_human= Methods.instance.RandomPosition(bg.gridPositions);
        }
        if (humanHint==1)
        {
            pos_human = LastestRed(reds);
            //make red log clear each turn?
           // GameManager.instance.redToken = "";
        }
        if (humanHint == 2)
        {
            pos_human = RedNearAnchor(reds);
            Debug.Log("I am the red near anchor");
        }
        if (humanHint==3)
        {
            pos_human = DensestRed(reds);
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
        //Debug.Log("shortdis" + shortDis);
        shortDis = Methods.instance.FindAdjForAnchor(new Vector3(x,y,0f));
        Debug.Log("shortdis" + shortDis);
        return shortDis;
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

    private Vector3 DensestRed(String red_token)
    {
        string[] red_tokens = red_token.Split('#');
        string[] posTemp;
        string[] posTemp1;
        int[] adjCounter = new int[red_tokens.Length - 1];
        int x;
        int y;
        int x1;
        int y1;
        Vector3 pos;
        if (red_tokens.Length>=3)
        {
            Debug.Log("redtokens is"+(red_tokens.Length-1));
            for (int l = 0; l < red_tokens.Length - 2; l++)//-1 is space
            {
                
                posTemp = red_tokens[l].Split('-');
                x = StringToInt(posTemp[0]);
                y = StringToInt(posTemp[1]);
                //Debug.Log("now red token"+l+x+y);
                for (int j=l+1; j< red_tokens.Length - 1;j++)
                {
                    
                    posTemp1 = red_tokens[j].Split('-');
                    x1 = StringToInt(posTemp1[0]);
                    y1 = StringToInt(posTemp1[1]);
                    //Debug.Log(" compare with red token" + j+x1+y1);
                    //Debug.Log("distance=" + Vector3.Distance(new Vector3(x, y, 0f), new Vector3(x1, y1, 0f)));
                    if (Vector3.Distance(new Vector3(x,y,0f),new Vector3(x1,y1,0f))<=GameParameters.instance.minAnchorDis)
                    {
                        //Debug.Log("distance="+ Vector3.Distance(new Vector3(x, y, 0f), new Vector3(x1, y1, 0f)));
                        //Debug.Log(GameParameters.instance.minAnchorDis);
                        adjCounter[l]++;
                        adjCounter[j]++;
                        //Debug.Log("adj"+adjCounter[l]);
                    }
                }
            }
            int maxAdj = adjCounter.Max();
            int p = Array.IndexOf(adjCounter, maxAdj);
            string[] denestRed = red_tokens[p].Split('-');
            x = StringToInt(denestRed[0]);
            y = StringToInt(denestRed[1]);
            pos = Methods.instance.FindAdjForTile(x, y);
            Debug.Log("dendest position is"+pos.x+pos.y+" and conuter is "+ maxAdj);
         
            return pos;
        }
        else
        {
           // Debug.Log("not enough reds");
            return LastestRed(red_token);
        }
    }

   





    private void FixedUpdate()
    {
        //Debug.Log("try to cheack the");
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
            if (Methods.instance.Contains(blockTile.transform.position, GameManager.instance.trueAnchorPos)&& GameManager.instance.firstBlock == false)
            {
                GameManager.instance.firstBlock = true;
            }
            //if(Methods.instance.IsPathBloked(GameManager.instance.path_current,blockTile.transform.position) && GameManager.instance.firstBlock==false)
            //{
            //    GameManager.instance.firstBlock = true;
            //}
           
        }

    }
}

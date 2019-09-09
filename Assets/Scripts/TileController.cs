/*
 * The TileController monitors human blocking on human’s turn
 */

using UnityEngine;

public class TileController : MonoBehaviour
{
    private static int blockCounter = 1;
    //private AutoHuma autoHuma;
    public BoardGenerator bg;
    private Vector3 pos;
    private TileController tl;
    private UIManager uI;

    public void AutoGrey(GameObject bt)
    //private void OnMouseDown()
    {



        SpriteRenderer sr = bt.GetComponent<SpriteRenderer>();
        Color color = new Color(43f, 54f, 58f);
        color.a = 0.1f;
        sr.color = color;
        //Debug.Log(pos + "clicked!");
        GameManager.instance.gameLog += "Player blocks " + transform.position + "\n";
        blockCounter++;
        //GameManager.instance.SetPlayerTurn(false);
        //StartCoroutine(GameManager.instance.TurnSwitch());
        Methods.instance.BlockTile(bt.transform.position);
        //GameObject blockTile = bg.tilePos[(int)pos.x, (int)pos.y];
        //blockTile.GetComponent<Transform>().position = pos;
        Debug.Log(bt.transform.position);

        if (blockCounter > GameParameters.instance.blocksPerTurn)
        {
            blockCounter = 1;
            GameManager.instance.SetPlayerTurn(false);
            StartCoroutine(GameManager.instance.TurnSwitch());
        }

    }

    private void FixedUpdate()
    {
        //Debug.Log("try to cheack the");
        if (!GameManager.instance.gameOver && GameManager.instance.playerTurn)
        {
            Debug.Log("is true?" + GameManager.instance.playerTurn);
            uI = GetComponent<UIManager>();
            uI.ShowPlayerTurn();
            bg = GetComponent<BoardGenerator>();
            tl = GetComponent<TileController>();
            pos = Methods.instance.RandomPosition(bg.gridPositions);
            GameObject blockTile = bg.tilePos[(int)pos.x, (int)pos.y];
            blockTile.GetComponent<Transform>().position = pos;
            while(Methods.instance.IsEmptyGrid(blockTile.transform.position))
            {
             
                tl.AutoGrey(blockTile);
            }
        }


        // autoHuma.AutoHuman();
    }
}










// private void OnMouseDown()
// {
//     if (!GameManager.instance.gameOver && GameManager.instance.playerTurn)
//     {
//         if (Methods.instance.IsEmptyGrid(transform.position))// another game stateform 
//         {
//             SpriteRenderer sr = GetComponent<SpriteRenderer>();
//             Color color = new Color(43f, 54f, 58f);
//             color.a = 0.1f;
//             sr.color = color;
//             Methods.instance.BlockTile(transform.position);
//             Debug.Log(transform.position + "clicked!");
//             GameManager.instance.gameLog += "Player blocks " + transform.position + "\n";
//	blockCounter++;
//             //GameManager.instance.SetPlayerTurn(false);
//             //StartCoroutine(GameManager.instance.TurnSwitch());
//         }
//if (blockCounter > GameParameters.instance.blocksPerTurn)
//{	
//	blockCounter = 1;
//	GameManager.instance.SetPlayerTurn(false);
//	StartCoroutine(GameManager.instance.TurnSwitch());
//}
//     }
// }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public float width;
    public float height;
    public float leftBound; //define all the boundaries within this function, as they will be different depending on the pivot point, and it would be nice to encapsulate this logic within the class as opposed to defining/maintaining it externally.
    public float rightBound;
    public float topBound;
    public float bottomBound;
    public PivotPoint pivot;
    public Sgs.SgsButtonHandler function;

    public enum PivotPoint
    {
        Center = 1,
        TopLeft = 0
    }

    void Start()
    {
        Text text = GetComponent<Text>();
        float x = transform.position.x;
        float y = transform.position.y;

        if (text != null)
        {
            //IMPORTANT: The following two lines of code will only work if there are no line breaks. For CCh, that is OK, but I will have to upgrade this in future games.
            width = text.message.Length * text.LETTER_WIDTH;
            height = text.LETTER_HEIGHT;
            if (pivot == PivotPoint.Center)
            {
                /*NOTE: I do not plan to enter this block for CCh, but if I do, I imagine the offsets on each of these will need to have the widths and heights divided by two and whatnot similar to the block below this.
                leftBound = x - width / 2;
                rightBound = x + width / 2;
                topBound = y - height / 2;
                bottomBound = y + height / 2;*/
            }
            else if (pivot == PivotPoint.TopLeft)
            {
                leftBound = x - text.LETTER_WIDTH / 2;
                rightBound = x + width - text.LETTER_WIDTH / 2;
                topBound = y - height / 2;
                bottomBound = y + height / 2;
            }
        }
    }

    void FixedUpdate()
    {
        
    }
}

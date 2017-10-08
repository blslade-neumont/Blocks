using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocks : MonoBehaviour
{
    [SerializeField] [Range(1, 40)] public int m_numColumns = 5;
    [SerializeField] [Range(1, 40)] public int m_numRows = 5;
    [SerializeField] public Transform m_sizeMin = null;
    [SerializeField] public Transform m_sizeMax = null;
    [SerializeField] public ObjectPool m_objectPool = null;
    
    public List<Block> m_blocks = new List<Block>();

    public void CreateBlocks(string pattern)
    {
        float width = Mathf.Abs(m_sizeMax.position.x - m_sizeMin.position.x);
        float height = Mathf.Abs(m_sizeMax.position.y - m_sizeMin.position.y);

        float sx = m_sizeMin.position.x;
        float sy = m_sizeMin.position.y;
        float dx = width / m_numColumns;
        float dy = height / m_numRows;

        for (int i = 0; i < m_numColumns; i++)
        {
            for (int j = 0; j < m_numRows; j++)
            {
                Vector3 position = Vector3.zero;
                position.x = sx + (dx * i) + (dx * 0.5f);
                position.y = sy + (dy * j);
                position.z = 0.0f;

                GameObject gameObject = m_objectPool.GetObject();
                gameObject.transform.parent = transform;
                Block block = gameObject.GetComponent<Block>();
                block.Initialize(position, Block.eType.STANDARD, this);
                
                m_blocks.Add(block);
            }
        }
    }

    public void RemoveBlock(Block block)
    {
        m_blocks.Remove(block);
        m_objectPool.ReturnObject(block.gameObject);
    }
}

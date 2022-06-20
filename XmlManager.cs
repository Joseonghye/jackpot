using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class XmlManager : MonoBehaviour {
    static XmlManager _instance;
    mNode[] m_node;
    static int nodeCount;
    public int getNodeCount() { return nodeCount; }
    public static  XmlManager Instance()
    {
        if (_instance == null)
        {
            _instance = GameObject.FindObjectOfType<XmlManager>();
            if(_instance == null)
            {
                Debug.Log("NOT INSTANCE!");
            }
        }
        return _instance;
    }
    void Start () {
        LoadXML();
        
    }
    
    void LoadXML()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(Application.dataPath + "/Resources/xml2.xml");

        //하나씩 가져올때
        XmlNodeList nodeTable = xmlDoc.GetElementsByTagName("field");
        //logTest(nodeTable);
        m_node = new mNode[nodeTable.Count];
        nodeCount = nodeTable.Count;
        int c = 0;
        for (int i = 0; i < nodeTable.Count; i++)
            m_node[i] = new mNode();
        foreach (XmlNode node in nodeTable)
        {
            if (node.Attributes.GetNamedItem("Id") != null) break;
            m_node[c].Id = (node.Attributes.GetNamedItem("Id").Value);
            m_node[c].ScriptType = (node.Attributes.GetNamedItem("ScriptType").Value);
            m_node[c].ScriptChName = (node.Attributes.GetNamedItem("ScriptChName").Value);
            m_node[c].ScriptChText = (node.Attributes.GetNamedItem("ScriptChText").Value);
            if (node.Attributes.GetNamedItem("FilmMove") != null)
                m_node[c].FilmMove = (node.Attributes.GetNamedItem("FilmMove").Value);
            if (node.Attributes.GetNamedItem("DeleteBlackArt") != null)
                m_node[c].DeleteBlackArt = (node.Attributes.GetNamedItem("DeleteBlackArt").Value);
            if (node.Attributes.GetNamedItem("EvidenceTrueCheck") != null)
                m_node[c].EvidenceTrueCheck = (node.Attributes.GetNamedItem("EvidenceTrueCheck").Value);
            if (node.Attributes.GetNamedItem("Evidence1st") != null)
                m_node[c].Evidence1st = (node.Attributes.GetNamedItem("Evidence1st").Value);
            if (node.Attributes.GetNamedItem("Evidence2st") != null)
                m_node[c].Evidence2st = (node.Attributes.GetNamedItem("Evidence2st").Value);
            if (node.Attributes.GetNamedItem("Evidence3st") != null)
                m_node[c].Evidence3st = (node.Attributes.GetNamedItem("Evidence3st").Value);
            if (node.Attributes.GetNamedItem("Evidence4st") != null)
                m_node[c].Evidence4st = (node.Attributes.GetNamedItem("Evidence4st").Value);
            if (node.Attributes.GetNamedItem("TrueScriptID") != null)
                m_node[c].TrueScriptID = (node.Attributes.GetNamedItem("TrueScriptID").Value);
            if (node.Attributes.GetNamedItem("FalseScriptID") != null)
                m_node[c].FalseScriptID = (node.Attributes.GetNamedItem("FalseScriptID").Value);
            Debug.Log("C = " + c);
            c++;
        }
        //logTest(nodeTable);
        //전체 가져올때
        XmlNodeList nodes = xmlDoc.SelectNodes("xml");
        foreach (XmlNode node in nodes)
        {
            Debug.Log("ID~" + node.SelectSingleNode("field").InnerText);//no
            Debug.Log("SCRIPTTYPE~" + node.SelectSingleNode("ScriptType").InnerText);
        }
    }
    public mNode getNode(int n)
    {
        return m_node[n];
    }
    void logTest(XmlNodeList nt)
    {
        foreach (XmlNode node in nt)
        {
            Debug.Log(node.Attributes.GetNamedItem("Id").Value);
            Debug.Log(node.Attributes.GetNamedItem("ScriptType").Value);
            Debug.Log(node.Attributes.GetNamedItem("ScriptChName").Value);
            Debug.Log(node.Attributes.GetNamedItem("ScriptChText").Value);
            //Debug.Log(node.Attributes.GetNamedItem("FilmMove").Value);
            //Debug.Log(node.Attributes.GetNamedItem("DeleteBlackArt").Value);
            //Debug.Log(node.Attributes.GetNamedItem("EvidenceTrueCheck").Value);
            //Debug.Log(node.Attributes.GetNamedItem("Evidence1st").Value);
            //Debug.Log(node.Attributes.GetNamedItem("Evidence2st").Value);
            //Debug.Log(node.Attributes.GetNamedItem("Evidence3st").Value);
            //Debug.Log(node.Attributes.GetNamedItem("Evidence4st").Value);
            //Debug.Log(node.Attributes.GetNamedItem("TrueScriptID").Value);
            //Debug.Log(node.Attributes.GetNamedItem("FalseScriptID").Value);
        }
    }
    void CreateXML()
    {
        XmlDocument doc = new XmlDocument();

        XmlDeclaration xmlDec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

        doc.AppendChild(xmlDec);

        XmlElement root = doc.CreateElement("");

        doc.AppendChild(root);



    }
}

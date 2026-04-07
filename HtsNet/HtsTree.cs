using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HtsNet
{
    //wip
    public class HtsQuestion
    {
        public string Identifier { get; set; }
        public string[] Patterns { get; set; }
        public bool Matches(string label)
        {
            foreach (var pattern in Patterns)
            {
                var patternForRegex = pattern.Replace("*", ".*").Replace("?", ".").Replace("^", "\\^").Replace("+", "\\+");
                if (Regex.IsMatch(label, patternForRegex))
                    return true;
            }
            return false;
        }
    }
    public class HtsTreeNode
    {
        public int Identifier { get; set; }
        public HtsQuestion Question { get; set; }
        public HtsTreeNode LeftNode { get; set; }
        public HtsTreeNode RightNode { get; set; }
    }
    public struct HtsTempNode
    {
        public HtsQuestion qs;
        public int left;
        public int right;
    }
    public class HtsTree
    {
        public List<HtsQuestion> Questions { get; set; } = new List<HtsQuestion>();
        public List<HtsTreeNode> RootNodes { get; set; } = new List<HtsTreeNode>();
        public HtsTree(string inf)
        {
            bool isDecisionTree = false;
            int currentState = 2;
            var tempTree = new Dictionary<int, HtsTempNode>();
            foreach (var line in inf.Split('\n'))
            {
                var data = line.Split(' ');
                if (data[0].Equals("QS"))
                {
                    var patterns = data[3].Split(',');
                    for (int i = 0; i < patterns.Length; i++)
                    {
                        patterns[i] = patterns[i].Replace("\"", "");
                    }
                    var qs = new HtsQuestion 
                    {
                        Identifier = data[1],
                        Patterns = patterns
                    };
                    Questions.Add(qs);
                }
                else if (data[0].Contains("{*}"))
                {
                    if (data[0].Contains(currentState.ToString()))
                        isDecisionTree = true;
                }
                else if (data[0].Equals("}"))
                {
                    int nodeId = tempTree.First().Key;
                    var tempNode = tempTree[nodeId];
                    var rootNode = new HtsTreeNode
                    {
                        Identifier = nodeId,
                        Question = tempNode.qs,
                        LeftNode = GetNodeFromTempTree(tempTree, tempNode.left),
                        RightNode = GetNodeFromTempTree(tempTree, tempNode.right),
                    };
                    RootNodes.Add(rootNode);
                    isDecisionTree = false;
                    currentState++;
                    tempTree.Clear();
                }
                else if (isDecisionTree)
                {
                    data = line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    if (data.Length == 4)
                    {
                        var id = int.Parse(data[0]);
                        var qs = Questions.Find(x => x.Identifier == data[1]);
                        var left = 0;
                        var right = 0;
                        if (data[2].Contains("_"))
                        {
                            data[2] = data[2].Replace("\"", "");
                            var split = data[2].Split('_');
                            left = int.Parse(split.Last());
                        }
                        else
                        {
                            left = int.Parse(data[2]);
                        }
                        if (data[3].Contains("_"))
                        {
                            data[3] = data[3].Replace("\"", "");
                            var split = data[3].Split('_');
                            right = int.Parse(split.Last());
                        }
                        else
                        {
                            right = int.Parse(data[3]);
                        }
                        var temp = new HtsTempNode
                        {
                            qs = qs,
                            left = left,
                            right = right
                        };
                        tempTree.Add(id, temp);
                    }
                }
            }
        }
        public HtsTreeNode GetNodeFromTempTree(Dictionary<int, HtsTempNode> tempTree, int id)
        {
            if (id <= 0)
            {
                var tempNode = tempTree[id];
                return new HtsTreeNode
                {
                    Identifier = id,
                    Question = tempNode.qs,
                    LeftNode = GetNodeFromTempTree(tempTree, tempNode.left),
                    RightNode = GetNodeFromTempTree(tempTree, tempNode.right)
                };
            }
            else
            {
                return new HtsTreeNode
                {
                    Identifier = id,
                    Question = null,
                    LeftNode = null,
                    RightNode = null
                };
            }
        }
    }
}

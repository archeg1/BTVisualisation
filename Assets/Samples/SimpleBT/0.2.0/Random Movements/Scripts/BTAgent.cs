using SimpleProto.AI.BehaviourTrees;
using UnityEngine;

public class BTAgent : MonoBehaviour
{
    private IBehaviourTreeNode _tree;
    public BTree behavior;
    
    private void Start()
    {
        var moveTarget = new Variable<Vector3>();
        var waitTime = new Variable<float>();

        //_tree = new RepeatForever
        //{
        //    new Sequence
        //    {
        //        new GetRandomPoint { Radius = 5, Output = moveTarget },
        //        new MoveTo { Target = new Vector3(3,5,4) },
        //        new GetRandomFloat { Min = 0.5f, Max = 2f, Output = waitTime },
        //        new Wait { Time = waitTime},
        //    }
        //};
        _tree = behavior.CreateRuntimeTree();
    }


    private void Update()
    {
        _tree.Execute(gameObject);
    }
}

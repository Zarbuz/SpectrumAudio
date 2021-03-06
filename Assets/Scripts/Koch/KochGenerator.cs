﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Initiator
{
    Triangle,
    Square,
    Pentagon,
    Hexagon,
    Heptagon,
    Octagon
};

public class KochGenerator : MonoBehaviour
{

    protected enum Axis
    {
        XAxis,
        YAxis,
        ZAxis
    };   

    public struct LineSegment
    {
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public Vector3 Direction;
        public float Length;
    }

    [System.Serializable]
    public struct StartGen
    {
        public bool Outwards;
        public float Scale;
    }

    public StartGen[] startGen;

    [SerializeField]
    protected Initiator initiator = Initiator.Triangle;

    [SerializeField]
    protected Axis axis = Axis.XAxis;

    [SerializeField]
    protected float initiatorSize;

    [SerializeField]
    protected AnimationCurve generator;

    [SerializeField]
    protected bool useBezierCurves;

    [SerializeField]
    [Range(8, 24)]
    protected int bezierVertexCount;

    protected Keyframe[] keys;
    protected int initiatorPointAmount;
    protected int generationCount;
    protected Vector3[] positions;
    protected Vector3[] targetPosition;
    protected Vector3[] bezierPosition;

    private Vector3[] _initiatorPoints;
    private Vector3 _rotateVector;
    private Vector3 _rotateAxis;
    private float _initialRotation;
    private List<LineSegment> _lineSegments;

    public float lengthOfSides;

    private void OnDrawGizmos()
    {
        GetInitiatorPoints();
        _initiatorPoints = new Vector3[initiatorPointAmount];
        _rotateVector = Quaternion.AngleAxis(_initialRotation, _rotateAxis) * _rotateVector;

        for (int i = 0; i < initiatorPointAmount; i++)
        {
            _initiatorPoints[i] = _rotateVector * initiatorSize;
            _rotateVector = Quaternion.AngleAxis(360 / initiatorPointAmount, _rotateAxis) * _rotateVector;
        }

        for (int i = 0; i < initiatorPointAmount; i++)
        {
            Gizmos.color = Color.white;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            if (i < initiatorPointAmount - 1)
            {
                Gizmos.DrawLine(_initiatorPoints[i], _initiatorPoints[i + 1]);
            }
            else
            {
                Gizmos.DrawLine(_initiatorPoints[i], _initiatorPoints[0]);
            }
        }

        lengthOfSides = Vector3.Distance(_initiatorPoints[0], _initiatorPoints[1]) * 0.5f;
    }

    private void Awake()
    {
        GetInitiatorPoints();
        _initiatorPoints = new Vector3[initiatorPointAmount];
        _rotateVector = Quaternion.AngleAxis(_initialRotation, _rotateAxis) * _rotateVector;
        positions = new Vector3[initiatorPointAmount + 1];
        targetPosition = new Vector3[initiatorPointAmount + 1];
        keys = generator.keys;
        _lineSegments = new List<LineSegment>();

        for (int i = 0; i < initiatorPointAmount; i++)
        {
            positions[i] = _rotateVector * initiatorSize;
            _rotateVector = Quaternion.AngleAxis(360 / initiatorPointAmount, _rotateAxis) * _rotateVector;
        }

        positions[initiatorPointAmount] = positions[0];
        targetPosition = positions;

        for (int i = 0; i < startGen.Length; i++)
        {
            Generate(targetPosition, startGen[i].Outwards, startGen[i].Scale);
        }
    }

    protected void Generate(Vector3[] positions, bool outwards, float generatorMultiplier)
    {
        //creating line segments
        _lineSegments.Clear();
        for (int i = 0; i < positions.Length - 1; i++)
        {
            LineSegment line = new LineSegment();
            line.StartPosition = positions[i];
            if (i == positions.Length - 1)
            {
                line.EndPosition = positions[0];
            }
            else
            {
                line.EndPosition = positions[i + 1];
            }
            line.Direction = (line.EndPosition - line.StartPosition).normalized;
            line.Length = Vector3.Distance(line.EndPosition, line.StartPosition);
            _lineSegments.Add(line);
        }

        //add the line segment points to a point array
        List<Vector3> newPos = new List<Vector3>();
        List<Vector3> targetPos = new List<Vector3>();

        foreach (LineSegment line in _lineSegments)
        {
            newPos.Add(line.StartPosition);
            targetPos.Add(line.StartPosition);

            for (int j = 1; j < keys.Length - 1; j++)
            {
                float moveAmount = line.Length * keys[j].time;
                float heightAmount = (line.Length * keys[j].value) * generatorMultiplier;
                Vector3 movePos = line.StartPosition + (line.Direction * moveAmount);
                Vector3 direction = outwards ? Quaternion.AngleAxis(-90, _rotateAxis) * line.Direction : Quaternion.AngleAxis(90, _rotateAxis) * line.Direction;
                newPos.Add(movePos);
                targetPos.Add(movePos + (direction * heightAmount));
            }
        }

        newPos.Add(_lineSegments[0].StartPosition);
        targetPos.Add(_lineSegments[0].StartPosition);

        this.positions = new Vector3[newPos.Count];
        targetPosition = new Vector3[targetPos.Count];
        this.positions = newPos.ToArray();
        targetPosition = targetPos.ToArray();
        bezierPosition = BezierCurve(targetPosition, bezierVertexCount);
        generationCount++;
    }

    protected Vector3[] BezierCurve(Vector3[] points, int vertexCount)
    {
        var pointList = new List<Vector3>();
        for (int i = 0; i < points.Length; i+=2)
        {
            if (i + 2 <= points.Length - 1)
            {
                for (float ratio = 0f; ratio <= 1f; ratio += 1.0f / vertexCount)
                {
                    var tangentLineVertex1 = Vector3.Lerp(points[i], points[i + 1], ratio);
                    var tangentLineVertex2 = Vector3.Lerp(points[i + 1], points[i + 2], ratio);
                    var bezierpoint = Vector3.Lerp(tangentLineVertex1, tangentLineVertex2, ratio);

                    pointList.Add(bezierpoint);
                }
            }
        }

        return pointList.ToArray();
    }

    private void GetInitiatorPoints()
    {
        switch (initiator)
        {
            case Initiator.Triangle:
                initiatorPointAmount = 3;
                _initialRotation = 0;
                break;
            case Initiator.Square:
                initiatorPointAmount = 4;
                _initialRotation = 45;
                break;
            case Initiator.Pentagon:
                initiatorPointAmount = 5;
                _initialRotation = 36;
                break;
            case Initiator.Hexagon:
                initiatorPointAmount = 6;
                _initialRotation = 30;
                break;
            case Initiator.Heptagon:
                initiatorPointAmount = 7;
                _initialRotation = 25.71428f;
                break;
            case Initiator.Octagon:
                initiatorPointAmount = 8;
                _initialRotation = 22.5f;
                break;
        }

        switch (axis)
        {
            case Axis.XAxis:
                _rotateVector = new Vector3(1, 0, 0);
                _rotateAxis = new Vector3(0, 0, 1);
                break;
            case Axis.YAxis:
                _rotateVector = new Vector3(0, 1, 0);
                _rotateAxis = new Vector3(1, 0, 0);
                break;
            case Axis.ZAxis:
                _rotateVector = new Vector3(0, 0, 1);
                _rotateAxis = new Vector3(0, 1, 0);
                break;
        }
    }
}

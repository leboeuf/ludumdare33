using UnityEngine;
using System;
using System.Collections.Generic;

public class AIInputController : CharacterController
{

    GameObject mMyGoal;
    GameObject mOtherGoal;

    GameObject mBall;

    public Difficulty AIDifficulty = Difficulty.Easy;
    float[] mDefensiveDelay = new float[(int)Difficulty.Count];
    float[] mOffensiveDelay = new float[(int)Difficulty.Count];
    float[] mBoostDelay = new float[(int)Difficulty.Count];

    private bool mDefensive;
    public bool Defensive
    {
        get { return mDefensive; }
        set
        {
            mDefensive = value;
            if (mDefensive)
                TargetPositionDelay = mDefensiveDelay[(int)AIDifficulty];
            else
                TargetPositionDelay = mOffensiveDelay[(int)AIDifficulty];
        }
    }

    Vector3 movementAxis = new Vector3(0f, 0f, 0f);
    bool wantToSwim = false;

    // Target position delay
    public float TargetPositionDelay { get; set; }
    List<Pair<Vector3, float>> targetPositionList = new List<Pair<Vector3, float>>();

    AIInputController()
    {
        mDefensiveDelay[(int)Difficulty.Easy] = 0.8f;
        mDefensiveDelay[(int)Difficulty.Medium] = 0.4f;
        mDefensiveDelay[(int)Difficulty.Hard] = 0.0f;

        mOffensiveDelay[(int)Difficulty.Easy] = 0.25f;
        mOffensiveDelay[(int)Difficulty.Medium] = 0.1f;
        mOffensiveDelay[(int)Difficulty.Hard] = 0.0f;

        mBoostDelay[(int)Difficulty.Easy] = 7.0f;
        mBoostDelay[(int)Difficulty.Medium] = 4.0f;
        mBoostDelay[(int)Difficulty.Hard] = 0.0f;

        Defensive = false;
    }
    
    protected override Vector3 GetMovementAxis()
    {
        return movementAxis;
    }

    protected override bool IsSwimming()
    {
        return wantToSwim;
    }

    void Start()
    {
        Team team = GetTeam();
        if(team == Team.Red)
        {
            mMyGoal = GameObject.Find("Goal Left");
            mOtherGoal = GameObject.Find("Goal Right");
        }
        else
        {
            mMyGoal = GameObject.Find("Goal Right");
            mOtherGoal = GameObject.Find("Goal Left");
        }
    }

    protected override void DerivedUpdate()
    {
        CleanupPositionHistory();

        if (mBall == null)
        {
            mBall = GameObject.Find("Ball(Clone)");
            UpdateStartPos();
        }
        else
        {
            if (Defensive)
            {
                UpdateDefensive();
            }
            else
            {
                UpdateOffensive();
            }
        }
    }

    void CleanupPositionHistory()
    {
        while(targetPositionList.Count > 0)
        {
            if (targetPositionList[0].Second + TargetPositionDelay < Time.time)
            {
                targetPositionList.RemoveAt(0);
            }
            else
            {
                break;
            }
        }
    }


    void UpdateOffensive()
    {
        float ballRadius = mBall.transform.lossyScale.x / 2;
        float characterRadius = this.GetComponent<CircleCollider2D>().radius;

        Vector3 ballToGoal = mOtherGoal.transform.position - mBall.transform.position;
        ballToGoal.Normalize();

        Vector3 ballEdgePosition = mBall.transform.position - ballToGoal * ballRadius;

        // Try to figure out if we are on the wrong side of the ball (intersection before getting to the target position)
        Vector3 closestIntersection = ClosestIntersection(mBall.transform.position.x, mBall.transform.position.y, ballRadius, this.transform.position, ballEdgePosition);

		float aiDistanceFromNet = Math.Abs(this.transform.position.x - mMyGoal.transform.position.x);
		float ballDistanceFromNet = Math.Abs(mBall.transform.position.x - mMyGoal.transform.position.x);

        // If we can have a straight hit on the ball, go for it!
        float diff = Mathf.Abs(closestIntersection.sqrMagnitude - ballEdgePosition.sqrMagnitude);
		if (diff < 0.01 || (aiDistanceFromNet < ballDistanceFromNet))
        {
            Vector3 targetPosition = mBall.transform.position - ballToGoal * (ballRadius - 0.2f + characterRadius);
            SetTargetPosition(targetPosition, true, true);
        }
        else
        {
            Vector3 ballToThisAI = this.transform.position - mBall.transform.position;
            Vector3 avoidanceDirection = Vector3.Cross(Vector3.forward, ballToThisAI);
            avoidanceDirection.Normalize();

            float offsetAvoidance = 1.0f;
            if ((this.transform.position.y < 0 && avoidanceDirection.y < 0) ||
                (this.transform.position.y > 0 && avoidanceDirection.y > 0))
            {
                offsetAvoidance *= -1;
            }
            Vector3 targetPosition = mBall.transform.position + avoidanceDirection * offsetAvoidance;

            SetTargetPosition(targetPosition, true, false);
        }
    }

    void UpdateStartPos()
    {
        float offsetFromGoal = 2f;

        Vector3 goalToOtherGoal = mOtherGoal.transform.position - mMyGoal.transform.position;
        goalToOtherGoal.Normalize();
        Vector3 targetPosition = mMyGoal.transform.position + goalToOtherGoal * offsetFromGoal;
		
		SetTargetPosition(targetPosition, false, false);
    }

    void UpdateDefensive()
    {
        float ballDistanceFromNet = Math.Abs(mBall.transform.position.x - mMyGoal.transform.position.x);
        float offsetFromGoal = ballDistanceFromNet / 2f;
        if (offsetFromGoal < 2)
            offsetFromGoal = 2f;

        Vector3 goalToBall = mBall.transform.position - mMyGoal.transform.position;
        goalToBall.Normalize();
        Vector3 targetPosition = mMyGoal.transform.position + goalToBall * offsetFromGoal;

        if ((targetPosition - transform.position).sqrMagnitude > 0.1f)
            SetTargetPosition(targetPosition, false, false);
        else
        {
            movementAxis.x = 0f;
            movementAxis.y = 0f;
        }
    }

	void SetTargetPosition(Vector3 targetPosition, bool boost, bool fullSpeed)
    {
        Debug.DrawLine(transform.position, targetPosition, Color.red);

        targetPositionList.Add(new Pair<Vector3, float>(targetPosition, Time.time));

        movementAxis = GetCurrentTargetPosition() - transform.position;

        if (movementAxis.sqrMagnitude > 1f)
            movementAxis.Normalize();

		if (!fullSpeed && movementAxis.sqrMagnitude < (0.05 * 0.05))
        {
            movementAxis.x = 0f;
            movementAxis.y = 0f;
        }

		if (boost) {
			if(movementAxis.sqrMagnitude <= (0.5*0.5))
			{
                if (mLastSwimTime + mBoostDelay[(int)AIDifficulty] < Time.time)
				    wantToSwim = true;
			}
		}

		if (fullSpeed) {
			movementAxis.Normalize();
		}
    }

    private Vector3 GetCurrentTargetPosition()
    {
        if (targetPositionList.Count > 0)
            return targetPositionList[0].First;
        return Vector3.zero;
    }

    //----------------------------------------------------------------------------------------------------------------
    // MATH UTILS
    public Vector3 ClosestIntersection(float cx, float cy, float radius, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector2 intersection1;
        Vector2 intersection2;
        int intersections = FindLineCircleIntersections(cx, cy, radius, lineStart, lineEnd, out intersection1, out intersection2);

        if (intersections == 1)
            return intersection1;//one intersection

        if (intersections == 2)
        {
            double dist1 = Distance(intersection1, lineStart);
            double dist2 = Distance(intersection2, lineStart);

            if (dist1 < dist2)
                return intersection1;
            else
                return intersection2;
        }

        return Vector3.zero;// no intersections at all
    }

    private double Distance(Vector2 p1, Vector2 p2)
    {
        return Math.Sqrt(Math.Pow(p2.x - p1.x, 2) + Math.Pow(p2.y - p1.y, 2));
    }

    // Find the points of intersection.
    private int FindLineCircleIntersections(float cx, float cy, float radius,
        Vector2 point1, Vector2 point2, out Vector2 intersection1, out Vector2 intersection2)
    {
        float dx, dy, A, B, C, det, t;

        dx = point2.x - point1.x;
        dy = point2.y - point1.y;

        A = dx * dx + dy * dy;
        B = 2 * (dx * (point1.x - cx) + dy * (point1.y - cy));
        C = (point1.x - cx) * (point1.x - cx) + (point1.y - cy) * (point1.y - cy) - radius * radius;

        det = B * B - 4 * A * C;
        if ((A <= 0.0000001) || (det < 0))
        {
            // No real solutions.
            intersection1 = new Vector2(float.NaN, float.NaN);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 0;
        }
        else if (det == 0)
        {
            // One solution.
            t = -B / (2 * A);
            intersection1 = new Vector2(point1.x + t * dx, point1.y + t * dy);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 1;
        }
        else
        {
            // Two solutions.
            t = (float)((-B + Math.Sqrt(det)) / (2 * A));
            intersection1 = new Vector2(point1.x + t * dx, point1.y + t * dy);
            t = (float)((-B - Math.Sqrt(det)) / (2 * A));
            intersection2 = new Vector2(point1.x + t * dx, point1.y + t * dy);
            return 2;
        }
    }

    public class Pair<T, U>
    {
        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }
    };
}
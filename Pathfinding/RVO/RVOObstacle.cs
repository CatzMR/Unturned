﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.RVO
{
	public abstract class RVOObstacle : MonoBehaviour
	{
		protected abstract void CreateObstacles();

		protected abstract bool ExecuteInEditor { get; }

		protected abstract bool LocalCoordinates { get; }

		protected abstract bool StaticObstacle { get; }

		protected abstract bool AreGizmosDirty();

		public void OnDrawGizmos()
		{
			this.OnDrawGizmos(false);
		}

		public void OnDrawGizmosSelected()
		{
			this.OnDrawGizmos(true);
		}

		public void OnDrawGizmos(bool selected)
		{
			this.gizmoDrawing = true;
			Gizmos.color = new Color(0.615f, 1f, 0.06f, (!selected) ? 0.7f : 1f);
			if (this.gizmoVerts == null || this.AreGizmosDirty() || this._obstacleMode != this.obstacleMode)
			{
				this._obstacleMode = this.obstacleMode;
				if (this.gizmoVerts == null)
				{
					this.gizmoVerts = new List<Vector3[]>();
				}
				else
				{
					this.gizmoVerts.Clear();
				}
				this.CreateObstacles();
			}
			Matrix4x4 matrix = this.GetMatrix();
			for (int i = 0; i < this.gizmoVerts.Count; i++)
			{
				Vector3[] array = this.gizmoVerts[i];
				int j = 0;
				int num = array.Length - 1;
				while (j < array.Length)
				{
					Gizmos.DrawLine(matrix.MultiplyPoint3x4(array[j]), matrix.MultiplyPoint3x4(array[num]));
					num = j++;
				}
				if (selected)
				{
					int k = 0;
					int num2 = array.Length - 1;
					while (k < array.Length)
					{
						Vector3 vector = matrix.MultiplyPoint3x4(array[num2]);
						Vector3 vector2 = matrix.MultiplyPoint3x4(array[k]);
						Vector3 vector3 = (vector + vector2) * 0.5f;
						Vector3 normalized = (vector2 - vector).normalized;
						if (!(normalized == Vector3.zero))
						{
							Vector3 vector4 = Vector3.Cross(Vector3.up, normalized);
							Gizmos.DrawLine(vector3, vector3 + vector4);
							Gizmos.DrawLine(vector3 + vector4, vector3 + vector4 * 0.5f + normalized * 0.5f);
							Gizmos.DrawLine(vector3 + vector4, vector3 + vector4 * 0.5f - normalized * 0.5f);
						}
						num2 = k++;
					}
				}
			}
			this.gizmoDrawing = false;
		}

		protected virtual Matrix4x4 GetMatrix()
		{
			if (this.LocalCoordinates)
			{
				return base.transform.localToWorldMatrix;
			}
			return Matrix4x4.identity;
		}

		public void OnDisable()
		{
			if (this.addedObstacles != null)
			{
				if (this.sim == null)
				{
					throw new Exception("This should not happen! Make sure you are not overriding the OnEnable function");
				}
				for (int i = 0; i < this.addedObstacles.Count; i++)
				{
					this.sim.RemoveObstacle(this.addedObstacles[i]);
				}
			}
		}

		public void OnEnable()
		{
			if (this.addedObstacles != null)
			{
				if (this.sim == null)
				{
					throw new Exception("This should not happen! Make sure you are not overriding the OnDisable function");
				}
				for (int i = 0; i < this.addedObstacles.Count; i++)
				{
					ObstacleVertex obstacleVertex = this.addedObstacles[i];
					ObstacleVertex obstacleVertex2 = obstacleVertex;
					do
					{
						obstacleVertex.layer = this.layer;
						obstacleVertex = obstacleVertex.next;
					}
					while (obstacleVertex != obstacleVertex2);
					this.sim.AddObstacle(this.addedObstacles[i]);
				}
			}
		}

		public void Start()
		{
			this.addedObstacles = new List<ObstacleVertex>();
			this.sourceObstacles = new List<Vector3[]>();
			this.prevUpdateMatrix = this.GetMatrix();
			this.CreateObstacles();
		}

		public void Update()
		{
			Matrix4x4 matrix = this.GetMatrix();
			if (matrix != this.prevUpdateMatrix)
			{
				for (int i = 0; i < this.addedObstacles.Count; i++)
				{
					this.sim.UpdateObstacle(this.addedObstacles[i], this.sourceObstacles[i], matrix);
				}
				this.prevUpdateMatrix = matrix;
			}
		}

		protected void FindSimulator()
		{
			RVOSimulator rvosimulator = Object.FindObjectOfType(typeof(RVOSimulator)) as RVOSimulator;
			if (rvosimulator == null)
			{
				throw new InvalidOperationException("No RVOSimulator could be found in the scene. Please add one to any GameObject");
			}
			this.sim = rvosimulator.GetSimulator();
		}

		protected void AddObstacle(Vector3[] vertices, float height)
		{
			if (vertices == null)
			{
				throw new ArgumentNullException("Vertices Must Not Be Null");
			}
			if (height < 0f)
			{
				throw new ArgumentOutOfRangeException("Height must be non-negative");
			}
			if (vertices.Length < 2)
			{
				throw new ArgumentException("An obstacle must have at least two vertices");
			}
			if (this.gizmoDrawing)
			{
				Vector3[] array = new Vector3[vertices.Length];
				this.WindCorrectly(vertices);
				Array.Copy(vertices, array, vertices.Length);
				this.gizmoVerts.Add(array);
				return;
			}
			if (this.sim == null)
			{
				this.FindSimulator();
			}
			if (vertices.Length == 2)
			{
				this.AddObstacleInternal(vertices, height);
				return;
			}
			this.WindCorrectly(vertices);
			this.AddObstacleInternal(vertices, height);
		}

		private void AddObstacleInternal(Vector3[] vertices, float height)
		{
			this.addedObstacles.Add(this.sim.AddObstacle(vertices, height, this.GetMatrix(), this.layer));
			this.sourceObstacles.Add(vertices);
		}

		private void WindCorrectly(Vector3[] vertices)
		{
			int num = 0;
			float num2 = float.PositiveInfinity;
			for (int i = 0; i < vertices.Length; i++)
			{
				if (vertices[i].x < num2)
				{
					num = i;
					num2 = vertices[i].x;
				}
			}
			if (Polygon.IsClockwise(vertices[(num - 1 + vertices.Length) % vertices.Length], vertices[num], vertices[(num + 1) % vertices.Length]))
			{
				if (this.obstacleMode == RVOObstacle.ObstacleVertexWinding.KeepOut)
				{
					Array.Reverse(vertices);
				}
			}
			else if (this.obstacleMode == RVOObstacle.ObstacleVertexWinding.KeepIn)
			{
				Array.Reverse(vertices);
			}
		}

		public RVOObstacle.ObstacleVertexWinding obstacleMode;

		public RVOLayer layer = RVOLayer.DefaultObstacle;

		protected Simulator sim;

		private List<ObstacleVertex> addedObstacles;

		private List<Vector3[]> sourceObstacles;

		private bool gizmoDrawing;

		private List<Vector3[]> gizmoVerts;

		private RVOObstacle.ObstacleVertexWinding _obstacleMode;

		private Matrix4x4 prevUpdateMatrix;

		public enum ObstacleVertexWinding
		{
			KeepOut,
			KeepIn
		}
	}
}

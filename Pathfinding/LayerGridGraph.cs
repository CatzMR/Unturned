﻿using System;
using Pathfinding.Serialization;
using Pathfinding.Serialization.JsonFx;
using UnityEngine;

namespace Pathfinding
{
	public class LayerGridGraph : GridGraph, IRaycastableGraph, IUpdatableGraph
	{
		public override void OnDestroy()
		{
			base.OnDestroy();
			this.RemoveGridGraphFromStatic();
		}

		public new void RemoveGridGraphFromStatic()
		{
			LevelGridNode.SetGridGraph(this.active.astarData.GetGraphIndex(this), null);
		}

		public override bool uniformWidthDepthGrid
		{
			get
			{
				return false;
			}
		}

		public override void GetNodes(GraphNodeDelegateCancelable del)
		{
			if (this.nodes == null)
			{
				return;
			}
			for (int i = 0; i < this.nodes.Length; i++)
			{
				if (this.nodes[i] != null && !del(this.nodes[i]))
				{
					break;
				}
			}
		}

		public new void UpdateArea(GraphUpdateObject o)
		{
			if (this.nodes == null || this.nodes.Length != this.width * this.depth * this.layerCount)
			{
				Debug.LogWarning("The Grid Graph is not scanned, cannot update area ");
				return;
			}
			Bounds bounds = o.bounds;
			Vector3 vector;
			Vector3 vector2;
			base.GetBoundsMinMax(bounds, this.inverseMatrix, out vector, out vector2);
			int xmin = Mathf.RoundToInt(vector.x - 0.5f);
			int xmax = Mathf.RoundToInt(vector2.x - 0.5f);
			int ymin = Mathf.RoundToInt(vector.z - 0.5f);
			int ymax = Mathf.RoundToInt(vector2.z - 0.5f);
			IntRect intRect = new IntRect(xmin, ymin, xmax, ymax);
			IntRect intRect2 = intRect;
			IntRect b = new IntRect(0, 0, this.width - 1, this.depth - 1);
			IntRect intRect3 = intRect;
			bool flag = o.updatePhysics || o.modifyWalkability;
			bool flag2 = o is LayerGridGraphUpdate && ((LayerGridGraphUpdate)o).recalculateNodes;
			bool preserveExistingNodes = !(o is LayerGridGraphUpdate) || ((LayerGridGraphUpdate)o).preserveExistingNodes;
			int num = (!o.updateErosion) ? 0 : this.erodeIterations;
			if (o.trackChangedNodes && flag2)
			{
				Debug.LogError("Cannot track changed nodes when creating or deleting nodes.\nWill not update LayerGridGraph");
				return;
			}
			if (o.updatePhysics && !o.modifyWalkability && this.collision.collisionCheck)
			{
				Vector3 vector3 = new Vector3(this.collision.diameter, 0f, this.collision.diameter) * 0.5f;
				vector -= vector3 * 1.02f;
				vector2 += vector3 * 1.02f;
				intRect3 = new IntRect(Mathf.RoundToInt(vector.x - 0.5f), Mathf.RoundToInt(vector.z - 0.5f), Mathf.RoundToInt(vector2.x - 0.5f), Mathf.RoundToInt(vector2.z - 0.5f));
				intRect2 = IntRect.Union(intRect3, intRect2);
			}
			if (flag || num > 0)
			{
				intRect2 = intRect2.Expand(num + 1);
			}
			IntRect intRect4 = IntRect.Intersection(intRect2, b);
			if (!flag2)
			{
				for (int i = intRect4.xmin; i <= intRect4.xmax; i++)
				{
					for (int j = intRect4.ymin; j <= intRect4.ymax; j++)
					{
						for (int k = 0; k < this.layerCount; k++)
						{
							o.WillUpdateNode(this.nodes[k * this.width * this.depth + j * this.width + i]);
						}
					}
				}
			}
			if (o.updatePhysics && !o.modifyWalkability)
			{
				this.collision.Initialize(this.matrix, this.nodeSize);
				intRect4 = IntRect.Intersection(intRect3, b);
				bool flag3 = false;
				for (int l = intRect4.xmin; l <= intRect4.xmax; l++)
				{
					for (int m = intRect4.ymin; m <= intRect4.ymax; m++)
					{
						flag3 |= this.RecalculateCell(l, m, preserveExistingNodes);
					}
				}
				for (int n = intRect4.xmin; n <= intRect4.xmax; n++)
				{
					for (int num2 = intRect4.ymin; num2 <= intRect4.ymax; num2++)
					{
						for (int num3 = 0; num3 < this.layerCount; num3++)
						{
							int num4 = num3 * this.width * this.depth + num2 * this.width + n;
							LevelGridNode levelGridNode = this.nodes[num4];
							if (levelGridNode != null)
							{
								this.CalculateConnections(this.nodes, levelGridNode, n, num2, num3);
							}
						}
					}
				}
			}
			intRect4 = IntRect.Intersection(intRect, b);
			for (int num5 = intRect4.xmin; num5 <= intRect4.xmax; num5++)
			{
				for (int num6 = intRect4.ymin; num6 <= intRect4.ymax; num6++)
				{
					for (int num7 = 0; num7 < this.layerCount; num7++)
					{
						int num8 = num7 * this.width * this.depth + num6 * this.width + num5;
						LevelGridNode levelGridNode2 = this.nodes[num8];
						if (levelGridNode2 != null)
						{
							if (flag)
							{
								levelGridNode2.Walkable = levelGridNode2.WalkableErosion;
								if (o.bounds.Contains((Vector3)levelGridNode2.position))
								{
									o.Apply(levelGridNode2);
								}
								levelGridNode2.WalkableErosion = levelGridNode2.Walkable;
							}
							else if (o.bounds.Contains((Vector3)levelGridNode2.position))
							{
								o.Apply(levelGridNode2);
							}
						}
					}
				}
			}
			if (flag && num == 0)
			{
				intRect4 = IntRect.Intersection(intRect2, b);
				for (int num9 = intRect4.xmin; num9 <= intRect4.xmax; num9++)
				{
					for (int num10 = intRect4.ymin; num10 <= intRect4.ymax; num10++)
					{
						for (int num11 = 0; num11 < this.layerCount; num11++)
						{
							int num12 = num11 * this.width * this.depth + num10 * this.width + num9;
							LevelGridNode levelGridNode3 = this.nodes[num12];
							if (levelGridNode3 != null)
							{
								this.CalculateConnections(this.nodes, levelGridNode3, num9, num10, num11);
							}
						}
					}
				}
			}
			else if (flag && num > 0)
			{
				IntRect a = IntRect.Union(intRect, intRect3).Expand(num);
				IntRect a2 = a.Expand(num);
				a = IntRect.Intersection(a, b);
				a2 = IntRect.Intersection(a2, b);
				for (int num13 = a2.xmin; num13 <= a2.xmax; num13++)
				{
					for (int num14 = a2.ymin; num14 <= a2.ymax; num14++)
					{
						for (int num15 = 0; num15 < this.layerCount; num15++)
						{
							int num16 = num15 * this.width * this.depth + num14 * this.width + num13;
							LevelGridNode levelGridNode4 = this.nodes[num16];
							if (levelGridNode4 != null)
							{
								bool walkable = levelGridNode4.Walkable;
								levelGridNode4.Walkable = levelGridNode4.WalkableErosion;
								if (!a.Contains(num13, num14))
								{
									levelGridNode4.TmpWalkable = walkable;
								}
							}
						}
					}
				}
				for (int num17 = a2.xmin; num17 <= a2.xmax; num17++)
				{
					for (int num18 = a2.ymin; num18 <= a2.ymax; num18++)
					{
						for (int num19 = 0; num19 < this.layerCount; num19++)
						{
							int num20 = num19 * this.width * this.depth + num18 * this.width + num17;
							LevelGridNode levelGridNode5 = this.nodes[num20];
							if (levelGridNode5 != null)
							{
								this.CalculateConnections(this.nodes, levelGridNode5, num17, num18, num19);
							}
						}
					}
				}
				this.ErodeWalkableArea(a2.xmin, a2.ymin, a2.xmax + 1, a2.ymax + 1);
				for (int num21 = a2.xmin; num21 <= a2.xmax; num21++)
				{
					for (int num22 = a2.ymin; num22 <= a2.ymax; num22++)
					{
						if (!a.Contains(num21, num22))
						{
							for (int num23 = 0; num23 < this.layerCount; num23++)
							{
								int num24 = num23 * this.width * this.depth + num22 * this.width + num21;
								LevelGridNode levelGridNode6 = this.nodes[num24];
								if (levelGridNode6 != null)
								{
									levelGridNode6.Walkable = levelGridNode6.TmpWalkable;
								}
							}
						}
					}
				}
				for (int num25 = a2.xmin; num25 <= a2.xmax; num25++)
				{
					for (int num26 = a2.ymin; num26 <= a2.ymax; num26++)
					{
						for (int num27 = 0; num27 < this.layerCount; num27++)
						{
							int num28 = num27 * this.width * this.depth + num26 * this.width + num25;
							LevelGridNode levelGridNode7 = this.nodes[num28];
							if (levelGridNode7 != null)
							{
								this.CalculateConnections(this.nodes, levelGridNode7, num25, num26, num27);
							}
						}
					}
				}
			}
		}

		public override void ScanInternal(OnScanStatus status)
		{
			this.scans++;
			if (this.nodeSize <= 0f)
			{
				return;
			}
			base.GenerateMatrix();
			if (this.width > 1024 || this.depth > 1024)
			{
				Debug.LogError("One of the grid's sides is longer than 1024 nodes");
				return;
			}
			this.SetUpOffsetsAndCosts();
			LevelGridNode.SetGridGraph(this.active.astarData.GetGraphIndex(this), this);
			this.maxClimb = Mathf.Clamp(this.maxClimb, 0f, this.characterHeight);
			LinkedLevelCell[] array = new LinkedLevelCell[this.width * this.depth];
			if (this.collision == null)
			{
				this.collision = new GraphCollision();
			}
			this.collision.Initialize(this.matrix, this.nodeSize);
			for (int i = 0; i < this.depth; i++)
			{
				for (int j = 0; j < this.width; j++)
				{
					array[i * this.width + j] = new LinkedLevelCell();
					LinkedLevelCell linkedLevelCell = array[i * this.width + j];
					Vector3 position = this.matrix.MultiplyPoint3x4(new Vector3((float)j + 0.5f, 0f, (float)i + 0.5f));
					RaycastHit[] array2 = this.collision.CheckHeightAll(position);
					for (int k = 0; k < array2.Length / 2; k++)
					{
						RaycastHit raycastHit = array2[k];
						array2[k] = array2[array2.Length - 1 - k];
						array2[array2.Length - 1 - k] = raycastHit;
					}
					if (array2.Length > 0)
					{
						LinkedLevelNode linkedLevelNode = null;
						for (int l = 0; l < array2.Length; l++)
						{
							LinkedLevelNode linkedLevelNode2 = new LinkedLevelNode();
							linkedLevelNode2.position = array2[l].point;
							if (linkedLevelNode != null && linkedLevelNode2.position.y - linkedLevelNode.position.y <= this.mergeSpanRange)
							{
								linkedLevelNode.position = linkedLevelNode2.position;
								linkedLevelNode.hit = array2[l];
								linkedLevelNode.walkable = this.collision.Check(linkedLevelNode2.position);
							}
							else
							{
								linkedLevelNode2.walkable = this.collision.Check(linkedLevelNode2.position);
								linkedLevelNode2.hit = array2[l];
								linkedLevelNode2.height = float.PositiveInfinity;
								if (linkedLevelCell.first == null)
								{
									linkedLevelCell.first = linkedLevelNode2;
									linkedLevelNode = linkedLevelNode2;
								}
								else
								{
									linkedLevelNode.next = linkedLevelNode2;
									linkedLevelNode.height = linkedLevelNode2.position.y - linkedLevelNode.position.y;
									linkedLevelNode = linkedLevelNode.next;
								}
							}
						}
					}
					else
					{
						linkedLevelCell.first = new LinkedLevelNode
						{
							position = position,
							height = float.PositiveInfinity,
							walkable = !this.collision.unwalkableWhenNoGround
						};
					}
				}
			}
			int num = 0;
			this.layerCount = 0;
			for (int m = 0; m < this.depth; m++)
			{
				for (int n = 0; n < this.width; n++)
				{
					LinkedLevelCell linkedLevelCell2 = array[m * this.width + n];
					LinkedLevelNode linkedLevelNode3 = linkedLevelCell2.first;
					int num2 = 0;
					do
					{
						num2++;
						num++;
						linkedLevelNode3 = linkedLevelNode3.next;
					}
					while (linkedLevelNode3 != null);
					this.layerCount = ((num2 <= this.layerCount) ? this.layerCount : num2);
				}
			}
			if (this.layerCount > 255)
			{
				Debug.LogError("Too many layers, a maximum of LevelGridNode.MaxLayerCount are allowed (found " + this.layerCount + ")");
				return;
			}
			this.nodes = new LevelGridNode[this.width * this.depth * this.layerCount];
			for (int num3 = 0; num3 < this.nodes.Length; num3++)
			{
				this.nodes[num3] = new LevelGridNode(this.active);
				this.nodes[num3].Penalty = this.initialPenalty;
			}
			int num4 = 0;
			float num5 = Mathf.Cos(this.maxSlope * 0.0174532924f);
			for (int num6 = 0; num6 < this.depth; num6++)
			{
				for (int num7 = 0; num7 < this.width; num7++)
				{
					LinkedLevelCell linkedLevelCell3 = array[num6 * this.width + num7];
					LinkedLevelNode linkedLevelNode4 = linkedLevelCell3.first;
					linkedLevelCell3.index = num4;
					int num8 = 0;
					int num9 = 0;
					do
					{
						LevelGridNode levelGridNode = this.nodes[num6 * this.width + num7 + this.width * this.depth * num9];
						levelGridNode.SetPosition((Int3)linkedLevelNode4.position);
						levelGridNode.Walkable = linkedLevelNode4.walkable;
						if (linkedLevelNode4.hit.normal != Vector3.zero && (this.penaltyAngle || num5 < 1f))
						{
							float num10 = Vector3.Dot(linkedLevelNode4.hit.normal.normalized, this.collision.up);
							if (this.penaltyAngle)
							{
								levelGridNode.Penalty += (uint)Mathf.RoundToInt((1f - num10) * this.penaltyAngleFactor);
							}
							if (num10 < num5)
							{
								levelGridNode.Walkable = false;
							}
						}
						levelGridNode.NodeInGridIndex = num6 * this.width + num7;
						if (linkedLevelNode4.height < this.characterHeight)
						{
							levelGridNode.Walkable = false;
						}
						levelGridNode.WalkableErosion = levelGridNode.Walkable;
						num4++;
						num8++;
						linkedLevelNode4 = linkedLevelNode4.next;
						num9++;
					}
					while (linkedLevelNode4 != null);
					while (num9 < this.layerCount)
					{
						this.nodes[num6 * this.width + num7 + this.width * this.depth * num9] = null;
						num9++;
					}
					linkedLevelCell3.count = num8;
				}
			}
			this.nodeCellIndices = new int[array.Length];
			for (int num11 = 0; num11 < this.depth; num11++)
			{
				for (int num12 = 0; num12 < this.width; num12++)
				{
					for (int num13 = 0; num13 < this.layerCount; num13++)
					{
						GraphNode node = this.nodes[num11 * this.width + num12 + this.width * this.depth * num13];
						this.CalculateConnections(this.nodes, node, num12, num11, num13);
					}
				}
			}
			uint graphIndex = (uint)this.active.astarData.GetGraphIndex(this);
			for (int num14 = 0; num14 < this.nodes.Length; num14++)
			{
				LevelGridNode levelGridNode2 = this.nodes[num14];
				if (levelGridNode2 != null)
				{
					this.UpdatePenalty(levelGridNode2);
					levelGridNode2.GraphIndex = graphIndex;
					if (!levelGridNode2.HasAnyGridConnections())
					{
						levelGridNode2.Walkable = false;
						levelGridNode2.WalkableErosion = levelGridNode2.Walkable;
					}
				}
			}
			this.ErodeWalkableArea(0, 0, this.width, this.depth);
		}

		public bool RecalculateCell(int x, int z, bool preserveExistingNodes)
		{
			LinkedLevelCell linkedLevelCell = new LinkedLevelCell();
			Vector3 position = this.matrix.MultiplyPoint3x4(new Vector3((float)x + 0.5f, 0f, (float)z + 0.5f));
			RaycastHit[] array = this.collision.CheckHeightAll(position);
			for (int i = 0; i < array.Length / 2; i++)
			{
				RaycastHit raycastHit = array[i];
				array[i] = array[array.Length - 1 - i];
				array[array.Length - 1 - i] = raycastHit;
			}
			bool result = false;
			if (array.Length > 0)
			{
				LinkedLevelNode linkedLevelNode = null;
				for (int j = 0; j < array.Length; j++)
				{
					LinkedLevelNode linkedLevelNode2 = new LinkedLevelNode();
					linkedLevelNode2.position = array[j].point;
					if (linkedLevelNode != null && linkedLevelNode2.position.y - linkedLevelNode.position.y <= this.mergeSpanRange)
					{
						linkedLevelNode.position = linkedLevelNode2.position;
						linkedLevelNode.hit = array[j];
						linkedLevelNode.walkable = this.collision.Check(linkedLevelNode2.position);
					}
					else
					{
						linkedLevelNode2.walkable = this.collision.Check(linkedLevelNode2.position);
						linkedLevelNode2.hit = array[j];
						linkedLevelNode2.height = float.PositiveInfinity;
						if (linkedLevelCell.first == null)
						{
							linkedLevelCell.first = linkedLevelNode2;
							linkedLevelNode = linkedLevelNode2;
						}
						else
						{
							linkedLevelNode.next = linkedLevelNode2;
							linkedLevelNode.height = linkedLevelNode2.position.y - linkedLevelNode.position.y;
							linkedLevelNode = linkedLevelNode.next;
						}
					}
				}
			}
			else
			{
				linkedLevelCell.first = new LinkedLevelNode
				{
					position = position,
					height = float.PositiveInfinity,
					walkable = !this.collision.unwalkableWhenNoGround
				};
			}
			uint graphIndex = (uint)this.active.astarData.GetGraphIndex(this);
			LinkedLevelNode linkedLevelNode3 = linkedLevelCell.first;
			int num = 0;
			int k = 0;
			for (;;)
			{
				if (k >= this.layerCount)
				{
					if (k + 1 > 255)
					{
						break;
					}
					this.AddLayers(1);
					result = true;
				}
				LevelGridNode levelGridNode = this.nodes[z * this.width + x + this.width * this.depth * k];
				if (levelGridNode == null || !preserveExistingNodes)
				{
					this.nodes[z * this.width + x + this.width * this.depth * k] = new LevelGridNode(this.active);
					levelGridNode = this.nodes[z * this.width + x + this.width * this.depth * k];
					levelGridNode.Penalty = this.initialPenalty;
					levelGridNode.GraphIndex = graphIndex;
					result = true;
				}
				levelGridNode.SetPosition((Int3)linkedLevelNode3.position);
				levelGridNode.Walkable = linkedLevelNode3.walkable;
				levelGridNode.WalkableErosion = levelGridNode.Walkable;
				if (linkedLevelNode3.hit.normal != Vector3.zero)
				{
					float num2 = Vector3.Dot(linkedLevelNode3.hit.normal.normalized, this.collision.up);
					if (this.penaltyAngle)
					{
						levelGridNode.Penalty += (uint)Mathf.RoundToInt((1f - num2) * this.penaltyAngleFactor);
					}
					float num3 = Mathf.Cos(this.maxSlope * 0.0174532924f);
					if (num2 < num3)
					{
						levelGridNode.Walkable = false;
					}
				}
				levelGridNode.NodeInGridIndex = z * this.width + x;
				if (linkedLevelNode3.height < this.characterHeight)
				{
					levelGridNode.Walkable = false;
				}
				num++;
				linkedLevelNode3 = linkedLevelNode3.next;
				k++;
				if (linkedLevelNode3 == null)
				{
					goto Block_14;
				}
			}
			Debug.LogError("Too many layers, a maximum of LevelGridNode.MaxLayerCount are allowed (required " + (k + 1) + ")");
			return result;
			Block_14:
			while (k < this.layerCount)
			{
				this.nodes[z * this.width + x + this.width * this.depth * k] = null;
				k++;
			}
			linkedLevelCell.count = num;
			return result;
		}

		public void AddLayers(int count)
		{
			int num = this.layerCount + count;
			if (num > 255)
			{
				Debug.LogError("Too many layers, a maximum of LevelGridNode.MaxLayerCount are allowed (required " + num + ")");
				return;
			}
			LevelGridNode[] array = this.nodes;
			this.nodes = new LevelGridNode[this.width * this.depth * num];
			for (int i = 0; i < array.Length; i++)
			{
				this.nodes[i] = array[i];
			}
			this.layerCount = num;
		}

		public virtual void UpdatePenalty(LevelGridNode node)
		{
			node.Penalty = 0u;
			node.Penalty = this.initialPenalty;
			if (this.penaltyPosition)
			{
				node.Penalty += (uint)Mathf.RoundToInt(((float)node.position.y - this.penaltyPositionOffset) * this.penaltyPositionFactor);
			}
		}

		public override void ErodeWalkableArea(int xmin, int zmin, int xmax, int zmax)
		{
			xmin = ((xmin >= 0) ? ((xmin <= this.width) ? xmin : this.width) : 0);
			xmax = ((xmax >= 0) ? ((xmax <= this.width) ? xmax : this.width) : 0);
			zmin = ((zmin >= 0) ? ((zmin <= this.depth) ? zmin : this.depth) : 0);
			zmax = ((zmax >= 0) ? ((zmax <= this.depth) ? zmax : this.depth) : 0);
			if (this.erosionUseTags)
			{
				Debug.LogError("Erosion Uses Tags is not supported for LayerGridGraphs yet");
			}
			for (int i = 0; i < this.erodeIterations; i++)
			{
				for (int j = 0; j < this.layerCount; j++)
				{
					for (int k = zmin; k < zmax; k++)
					{
						for (int l = xmin; l < xmax; l++)
						{
							LevelGridNode levelGridNode = this.nodes[k * this.width + l + this.width * this.depth * j];
							if (levelGridNode != null)
							{
								if (levelGridNode.Walkable)
								{
									bool flag = false;
									for (int m = 0; m < 4; m++)
									{
										if (!levelGridNode.GetConnection(m))
										{
											flag = true;
											break;
										}
									}
									if (flag)
									{
										levelGridNode.Walkable = false;
									}
								}
							}
						}
					}
				}
				for (int n = 0; n < this.layerCount; n++)
				{
					for (int num = zmin; num < zmax; num++)
					{
						for (int num2 = xmin; num2 < xmax; num2++)
						{
							LevelGridNode levelGridNode2 = this.nodes[num * this.width + num2 + this.width * this.depth * n];
							if (levelGridNode2 != null)
							{
								this.CalculateConnections(this.nodes, levelGridNode2, num2, num, n);
							}
						}
					}
				}
			}
		}

		public void CalculateConnections(GraphNode[] nodes, GraphNode node, int x, int z, int layerIndex)
		{
			if (node == null)
			{
				return;
			}
			LevelGridNode levelGridNode = (LevelGridNode)node;
			levelGridNode.ResetAllGridConnections();
			if (!node.Walkable)
			{
				return;
			}
			float num;
			if (layerIndex == this.layerCount - 1 || nodes[levelGridNode.NodeInGridIndex + this.width * this.depth * (layerIndex + 1)] == null)
			{
				num = float.PositiveInfinity;
			}
			else
			{
				num = (float)Math.Abs(levelGridNode.position.y - nodes[levelGridNode.NodeInGridIndex + this.width * this.depth * (layerIndex + 1)].position.y) * 0.001f;
			}
			for (int i = 0; i < 4; i++)
			{
				int num2 = x + this.neighbourXOffsets[i];
				int num3 = z + this.neighbourZOffsets[i];
				if (num2 >= 0 && num3 >= 0 && num2 < this.width && num3 < this.depth)
				{
					int num4 = num3 * this.width + num2;
					int value = 255;
					for (int j = 0; j < this.layerCount; j++)
					{
						GraphNode graphNode = nodes[num4 + this.width * this.depth * j];
						if (graphNode != null && graphNode.Walkable)
						{
							float num5;
							if (j == this.layerCount - 1 || nodes[num4 + this.width * this.depth * (j + 1)] == null)
							{
								num5 = float.PositiveInfinity;
							}
							else
							{
								num5 = (float)Math.Abs(graphNode.position.y - nodes[num4 + this.width * this.depth * (j + 1)].position.y) * 0.001f;
							}
							float num6 = Mathf.Max((float)graphNode.position.y * 0.001f, (float)levelGridNode.position.y * 0.001f);
							float num7 = Mathf.Min((float)graphNode.position.y * 0.001f + num5, (float)levelGridNode.position.y * 0.001f + num);
							float num8 = num7 - num6;
							if (num8 >= this.characterHeight && (float)Mathf.Abs(graphNode.position.y - levelGridNode.position.y) * 0.001f <= this.maxClimb)
							{
								value = j;
							}
						}
					}
					levelGridNode.SetConnectionValue(i, value);
				}
			}
		}

		public override NNInfo GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint = null)
		{
			if (this.nodes == null || this.depth * this.width * this.layerCount != this.nodes.Length)
			{
				return default(NNInfo);
			}
			position = this.inverseMatrix.MultiplyPoint3x4(position);
			int num = Mathf.Clamp(Mathf.RoundToInt(position.x - 0.5f), 0, this.width - 1);
			int num2 = Mathf.Clamp(Mathf.RoundToInt(position.z - 0.5f), 0, this.depth - 1);
			int num3 = this.width * num2 + num;
			float num4 = float.PositiveInfinity;
			LevelGridNode node = null;
			for (int i = 0; i < this.layerCount; i++)
			{
				LevelGridNode levelGridNode = this.nodes[num3 + this.width * this.depth * i];
				if (levelGridNode != null)
				{
					float sqrMagnitude = ((Vector3)levelGridNode.position - position).sqrMagnitude;
					if (sqrMagnitude < num4)
					{
						num4 = sqrMagnitude;
						node = levelGridNode;
					}
				}
			}
			return new NNInfo(node);
		}

		private LevelGridNode GetNearestNode(Vector3 position, int x, int z, NNConstraint constraint)
		{
			int num = this.width * z + x;
			float num2 = float.PositiveInfinity;
			LevelGridNode result = null;
			for (int i = 0; i < this.layerCount; i++)
			{
				LevelGridNode levelGridNode = this.nodes[num + this.width * this.depth * i];
				if (levelGridNode != null)
				{
					float sqrMagnitude = ((Vector3)levelGridNode.position - position).sqrMagnitude;
					if (sqrMagnitude < num2 && constraint.Suitable(levelGridNode))
					{
						num2 = sqrMagnitude;
						result = levelGridNode;
					}
				}
			}
			return result;
		}

		public override NNInfo GetNearestForce(Vector3 position, NNConstraint constraint)
		{
			if (this.nodes == null || this.depth * this.width * this.layerCount != this.nodes.Length || this.layerCount == 0)
			{
				return default(NNInfo);
			}
			Vector3 vector = position;
			position = this.inverseMatrix.MultiplyPoint3x4(position);
			int num = Mathf.Clamp(Mathf.RoundToInt(position.x - 0.5f), 0, this.width - 1);
			int num2 = Mathf.Clamp(Mathf.RoundToInt(position.z - 0.5f), 0, this.depth - 1);
			float num3 = float.PositiveInfinity;
			int num4 = 2;
			LevelGridNode levelGridNode = this.GetNearestNode(vector, num, num2, constraint);
			if (levelGridNode != null)
			{
				num3 = ((Vector3)levelGridNode.position - vector).sqrMagnitude;
			}
			if (levelGridNode != null)
			{
				if (num4 == 0)
				{
					return new NNInfo(levelGridNode);
				}
				num4--;
			}
			float num5 = (!constraint.constrainDistance) ? float.PositiveInfinity : AstarPath.active.maxNearestNodeDistance;
			float num6 = num5 * num5;
			int num7 = 1;
			for (;;)
			{
				int i = num2 + num7;
				if (this.nodeSize * (float)num7 > num5)
				{
					break;
				}
				int j;
				for (j = num - num7; j <= num + num7; j++)
				{
					if (j >= 0 && i >= 0 && j < this.width && i < this.depth)
					{
						LevelGridNode nearestNode = this.GetNearestNode(vector, j, i, constraint);
						if (nearestNode != null)
						{
							float sqrMagnitude = ((Vector3)nearestNode.position - vector).sqrMagnitude;
							if (sqrMagnitude < num3 && sqrMagnitude < num6)
							{
								num3 = sqrMagnitude;
								levelGridNode = nearestNode;
							}
						}
					}
				}
				i = num2 - num7;
				for (j = num - num7; j <= num + num7; j++)
				{
					if (j >= 0 && i >= 0 && j < this.width && i < this.depth)
					{
						LevelGridNode nearestNode2 = this.GetNearestNode(vector, j, i, constraint);
						if (nearestNode2 != null)
						{
							float sqrMagnitude2 = ((Vector3)nearestNode2.position - vector).sqrMagnitude;
							if (sqrMagnitude2 < num3 && sqrMagnitude2 < num6)
							{
								num3 = sqrMagnitude2;
								levelGridNode = nearestNode2;
							}
						}
					}
				}
				j = num - num7;
				i = num2 - num7 + 1;
				for (i = num2 - num7 + 1; i <= num2 + num7 - 1; i++)
				{
					if (j >= 0 && i >= 0 && j < this.width && i < this.depth)
					{
						LevelGridNode nearestNode3 = this.GetNearestNode(vector, j, i, constraint);
						if (nearestNode3 != null)
						{
							float sqrMagnitude3 = ((Vector3)nearestNode3.position - vector).sqrMagnitude;
							if (sqrMagnitude3 < num3 && sqrMagnitude3 < num6)
							{
								num3 = sqrMagnitude3;
								levelGridNode = nearestNode3;
							}
						}
					}
				}
				j = num + num7;
				for (i = num2 - num7 + 1; i <= num2 + num7 - 1; i++)
				{
					if (j >= 0 && i >= 0 && j < this.width && i < this.depth)
					{
						LevelGridNode nearestNode4 = this.GetNearestNode(vector, j, i, constraint);
						if (nearestNode4 != null)
						{
							float sqrMagnitude4 = ((Vector3)nearestNode4.position - vector).sqrMagnitude;
							if (sqrMagnitude4 < num3 && sqrMagnitude4 < num6)
							{
								num3 = sqrMagnitude4;
								levelGridNode = nearestNode4;
							}
						}
					}
				}
				if (levelGridNode != null)
				{
					if (num4 == 0)
					{
						goto Block_37;
					}
					num4--;
				}
				num7++;
			}
			return new NNInfo(levelGridNode);
			Block_37:
			return new NNInfo(levelGridNode);
		}

		public new bool Linecast(Vector3 _a, Vector3 _b)
		{
			GraphHitInfo graphHitInfo;
			return this.Linecast(_a, _b, null, out graphHitInfo);
		}

		public new bool Linecast(Vector3 _a, Vector3 _b, GraphNode hint)
		{
			GraphHitInfo graphHitInfo;
			return this.Linecast(_a, _b, hint, out graphHitInfo);
		}

		public new bool Linecast(Vector3 _a, Vector3 _b, GraphNode hint, out GraphHitInfo hit)
		{
			return this.SnappedLinecast(_a, _b, hint, out hit);
		}

		public new bool SnappedLinecast(Vector3 _a, Vector3 _b, GraphNode hint, out GraphHitInfo hit)
		{
			hit = default(GraphHitInfo);
			LevelGridNode levelGridNode = base.GetNearest(_a, NNConstraint.None).node as LevelGridNode;
			LevelGridNode levelGridNode2 = base.GetNearest(_b, NNConstraint.None).node as LevelGridNode;
			if (levelGridNode == null || levelGridNode2 == null)
			{
				hit.node = null;
				hit.point = _a;
				return true;
			}
			_a = this.inverseMatrix.MultiplyPoint3x4((Vector3)levelGridNode.position);
			_a.x -= 0.5f;
			_a.z -= 0.5f;
			_b = this.inverseMatrix.MultiplyPoint3x4((Vector3)levelGridNode2.position);
			_b.x -= 0.5f;
			_b.z -= 0.5f;
			Int3 ob = new Int3(Mathf.RoundToInt(_a.x), Mathf.RoundToInt(_a.y), Mathf.RoundToInt(_a.z));
			Int3 @int = new Int3(Mathf.RoundToInt(_b.x), Mathf.RoundToInt(_b.y), Mathf.RoundToInt(_b.z));
			hit.origin = (Vector3)ob;
			if (!levelGridNode.Walkable)
			{
				hit.node = levelGridNode;
				hit.point = this.matrix.MultiplyPoint3x4(new Vector3((float)ob.x + 0.5f, 0f, (float)ob.z + 0.5f));
				hit.point.y = ((Vector3)hit.node.position).y;
				return true;
			}
			int num = Mathf.Abs(ob.x - @int.x);
			int num2 = Mathf.Abs(ob.z - @int.z);
			LevelGridNode levelGridNode4;
			for (LevelGridNode levelGridNode3 = levelGridNode; levelGridNode3 != levelGridNode2; levelGridNode3 = levelGridNode4)
			{
				if (levelGridNode3.NodeInGridIndex == levelGridNode2.NodeInGridIndex)
				{
					hit.node = levelGridNode3;
					hit.point = (Vector3)levelGridNode3.position;
					return true;
				}
				num = Math.Abs(ob.x - @int.x);
				num2 = Math.Abs(ob.z - @int.z);
				int num3 = 0;
				if (num >= num2)
				{
					num3 = ((@int.x <= ob.x) ? 3 : 1);
				}
				else if (num2 > num)
				{
					num3 = ((@int.z <= ob.z) ? 0 : 2);
				}
				if (!this.CheckConnection(levelGridNode3, num3))
				{
					hit.node = levelGridNode3;
					hit.point = (Vector3)levelGridNode3.position;
					return true;
				}
				levelGridNode4 = this.nodes[levelGridNode3.NodeInGridIndex + this.neighbourOffsets[num3] + this.width * this.depth * levelGridNode3.GetConnectionValue(num3)];
				if (!levelGridNode4.Walkable)
				{
					hit.node = levelGridNode4;
					hit.point = (Vector3)levelGridNode4.position;
					return true;
				}
				ob = (Int3)this.inverseMatrix.MultiplyPoint3x4((Vector3)levelGridNode4.position);
			}
			return false;
		}

		public bool CheckConnection(LevelGridNode node, int dir)
		{
			return node.GetConnection(dir);
		}

		public override void OnDrawGizmos(bool drawNodes)
		{
			if (!drawNodes)
			{
				return;
			}
			base.OnDrawGizmos(false);
			if (this.nodes == null)
			{
				return;
			}
			PathHandler debugPathData = AstarPath.active.debugPathData;
			for (int i = 0; i < this.nodes.Length; i++)
			{
				LevelGridNode levelGridNode = this.nodes[i];
				if (levelGridNode != null && levelGridNode.Walkable)
				{
					Gizmos.color = this.NodeColor(levelGridNode, AstarPath.active.debugPathData);
					if (AstarPath.active.showSearchTree && AstarPath.active.debugPathData != null)
					{
						if (base.InSearchTree(levelGridNode, AstarPath.active.debugPath))
						{
							PathNode pathNode = debugPathData.GetPathNode(levelGridNode);
							if (pathNode != null && pathNode.parent != null)
							{
								Gizmos.DrawLine((Vector3)levelGridNode.position, (Vector3)pathNode.parent.node.position);
							}
						}
					}
					else
					{
						for (int j = 0; j < 4; j++)
						{
							int connectionValue = levelGridNode.GetConnectionValue(j);
							if (connectionValue != 255)
							{
								int num = levelGridNode.NodeInGridIndex + this.neighbourOffsets[j] + this.width * this.depth * connectionValue;
								if (num >= 0 && num <= this.nodes.Length)
								{
									GraphNode graphNode = this.nodes[num];
									if (graphNode != null)
									{
										Gizmos.DrawLine((Vector3)levelGridNode.position, (Vector3)graphNode.position);
									}
								}
							}
						}
					}
				}
			}
		}

		public override void SerializeExtraInfo(GraphSerializationContext ctx)
		{
			if (this.nodes == null)
			{
				ctx.writer.Write(-1);
				return;
			}
			ctx.writer.Write(this.nodes.Length);
			for (int i = 0; i < this.nodes.Length; i++)
			{
				if (this.nodes[i] == null)
				{
					ctx.writer.Write(-1);
				}
				else
				{
					ctx.writer.Write(0);
					this.nodes[i].SerializeNode(ctx);
				}
			}
		}

		public override void DeserializeExtraInfo(GraphSerializationContext ctx)
		{
			int num = ctx.reader.ReadInt32();
			if (num == -1)
			{
				this.nodes = null;
				return;
			}
			this.nodes = new LevelGridNode[num];
			for (int i = 0; i < this.nodes.Length; i++)
			{
				if (ctx.reader.ReadInt32() != -1)
				{
					this.nodes[i] = new LevelGridNode(this.active);
					this.nodes[i].DeserializeNode(ctx);
				}
				else
				{
					this.nodes[i] = null;
				}
			}
		}

		public override void PostDeserialization()
		{
			base.GenerateMatrix();
			this.SetUpOffsetsAndCosts();
			if (this.nodes == null || this.nodes.Length == 0)
			{
				return;
			}
			LevelGridNode.SetGridGraph(AstarPath.active.astarData.GetGraphIndex(this), this);
			for (int i = 0; i < this.depth; i++)
			{
				for (int j = 0; j < this.width; j++)
				{
					for (int k = 0; k < this.layerCount; k++)
					{
						LevelGridNode levelGridNode = this.nodes[i * this.width + j + this.width * this.depth * k];
						if (levelGridNode != null)
						{
							levelGridNode.NodeInGridIndex = i * this.width + j;
						}
					}
				}
			}
		}

		public int[] nodeCellIndices;

		[JsonMember]
		public int layerCount;

		[JsonMember]
		public float mergeSpanRange = 0.5f;

		[JsonMember]
		public float characterHeight = 0.4f;

		public new LevelGridNode[] nodes;
	}
}

﻿using System;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage
{
	public struct FoliageInstanceGroup
	{
		public FoliageInstanceGroup(AssetReference<FoliageInstancedMeshInfoAsset> newAssetReference, Matrix4x4 newMatrix, bool newClearWhenBaked)
		{
			this.assetReference = newAssetReference;
			this.matrix = newMatrix;
			this.clearWhenBaked = newClearWhenBaked;
		}

		public AssetReference<FoliageInstancedMeshInfoAsset> assetReference;

		public Matrix4x4 matrix;

		public bool clearWhenBaked;
	}
}

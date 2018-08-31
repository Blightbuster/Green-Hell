using System;
using UnityEngine;

namespace UltimateWater
{
	public class ShaderVariables
	{
		public static int LocalDisplacementMap
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._LocalDisplacementMap, "_LocalDisplacementMap");
			}
		}

		public static int LocalNormalMap
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._LocalNormalMap, "_LocalNormalMap");
			}
		}

		public static int LocalDiffuseMap
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._LocalDiffuseMap, "_LocalDiffuseMap");
			}
		}

		public static int LocalMapsCoords
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._LocalMapsCoords, "_LocalMapsCoords");
			}
		}

		public static int LocalMapsCoordsPrevious
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._LocalMapsCoordsPrevious, "_LocalMapsCoordsPrevious");
			}
		}

		public static int UnderwaterMask
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._UnderwaterMask, "_UnderwaterMask");
			}
		}

		public static int UnderwaterMask2
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._UnderwaterMask2, "_UnderwaterMask2");
			}
		}

		public static int AdditiveMask
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._AdditiveMask, "_AdditiveMask");
			}
		}

		public static int SubtractiveMask
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._SubtractiveMask, "_SubtractiveMask");
			}
		}

		public static int DisplacementsMask
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._DisplacementsMask, "_DisplacementsMask");
			}
		}

		public static int Gbuffer0
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._Gbuffer0, "_CameraGBufferTextureOriginal0");
			}
		}

		public static int Gbuffer1
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._Gbuffer1, "_CameraGBufferTextureOriginal1");
			}
		}

		public static int Gbuffer2
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._Gbuffer2, "_CameraGBufferTextureOriginal2");
			}
		}

		public static int Gbuffer3
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._Gbuffer3, "_CameraGBufferTextureOriginal3");
			}
		}

		public static int OcclusionMap
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._OcclusionMap, "_OcclusionMap");
			}
		}

		public static int OcclusionMap2
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._OcclusionMap2, "_OcclusionMap2");
			}
		}

		public static int OcclusionMapProjection
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._OcclusionMapProjection, "_OcclusionMapProjection");
			}
		}

		public static int FoamParameters
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._FoamParameters, "_FoamParameters");
			}
		}

		public static int FoamShoreIntensity
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._FoamShoreIntensity, "_FoamShoreIntensity");
			}
		}

		public static int FoamIntensity
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._FoamIntensity, "_FoamIntensity");
			}
		}

		public static int WaterTileSizeInvSrt
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._WaterTileSizeInvSrt, "_WaterTileSizeInvSRT");
			}
		}

		public static int[] DisplacementDeltaMaps
		{
			get
			{
				if (ShaderVariables._DisplacementDeltaMaps == null)
				{
					ShaderVariables._DisplacementDeltaMaps = new int[]
					{
						-1,
						-1,
						-1,
						-1
					};
					ShaderVariables.Cache(ref ShaderVariables._DisplacementDeltaMaps[0], "_DisplacementDeltaMap");
					ShaderVariables.Cache(ref ShaderVariables._DisplacementDeltaMaps[1], "_DisplacementDeltaMap1");
					ShaderVariables.Cache(ref ShaderVariables._DisplacementDeltaMaps[2], "_DisplacementDeltaMap2");
					ShaderVariables.Cache(ref ShaderVariables._DisplacementDeltaMaps[3], "_DisplacementDeltaMap3");
				}
				return ShaderVariables._DisplacementDeltaMaps;
			}
		}

		public static int WaterTileSize
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._WaterTileSize, "_WaterTileSize");
			}
		}

		public static int WaterTileSizeInv
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._WaterTileSizeInv, "_WaterTileSizeInv");
			}
		}

		public static int WaterTileSizeScales
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._WaterTileSizeScales, "_WaterTileSizeScales");
			}
		}

		public static int MaxDisplacement
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._MaxDisplacement, "_MaxDisplacement");
			}
		}

		public static int TotalDisplacementMap
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._TotalDisplacementMap, "_TotalDisplacementMap");
			}
		}

		public static int DisplacementMask
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._DisplacementMask, "_DisplacementsMask");
			}
		}

		public static int WaterDepthTexture
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._WaterDepthTexture, "_WaterDepthTexture");
			}
		}

		public static int UnityMatrixVPInverse
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._UnityMatrixVPInverse, "UNITY_MATRIX_VP_INVERSE");
			}
		}

		public static int DepthClipMultiplier
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._DepthClipMultiplier, "_DepthClipMultiplier");
			}
		}

		public static int RefractTex
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._RefractTex, "_RefractionTex");
			}
		}

		public static int WaterlessDepthTexture
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._WaterlessDepthTexture, "_WaterlessDepthTexture");
			}
		}

		public static int CameraDepthTexture2
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._CameraDepthTexture2, "_CameraDepthTexture2");
			}
		}

		public static int Velocity
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._Velocity, "Velocity");
			}
		}

		public static int WaterShadowmap
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._WaterShadowmap, "_WaterShadowmap");
			}
		}

		public static int PlanarReflectionTex
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._PlanarReflectionTex, "_PlanarReflectionTex");
			}
		}

		public static int ButterflyTex
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._ButterflyTex, "_ButterflyTex");
			}
		}

		public static int ButterflyPass
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._ButterflyPass, "_ButterflyPass");
			}
		}

		public static int Offset
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._Offset, "_Offset");
			}
		}

		public static int MaxDistance
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._MaxDistance, "_MaxDistance");
			}
		}

		public static int AbsorptionColorPerPixel
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._AbsorptionColorPerPixel, "_AbsorptionColorPerPixel");
			}
		}

		public static int SurfaceOffset
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._SurfaceOffset, "_SurfaceOffset");
			}
		}

		public static int WaterId
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._WaterId, "_WaterId");
			}
		}

		public static int Cull
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._Cull, "_Cull");
			}
		}

		public static int Period
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._Period, "_Period");
			}
		}

		public static int BumpMapST
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._BumpMapST, "_BumpMap_ST");
			}
		}

		public static int DetailAlbedoMapST
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._DetailAlbedoMapST, "_DetailAlbedoMap_ST");
			}
		}

		public static int HeightTex
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._HeightTex, "_HeightTex");
			}
		}

		public static int DisplacementTex
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._DisplacementTex, "_DisplacementTex");
			}
		}

		public static int HorizontalDisplacementScale
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._HorizontalDisplacementScale, "_HorizontalDisplacementScale");
			}
		}

		public static int JacobianScale
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._JacobianScale, "_JacobianScale");
			}
		}

		public static int RenderTime
		{
			get
			{
				return ShaderVariables.Cache(ref ShaderVariables._RenderTime, "_RenderTime");
			}
		}

		private static int Cache(ref int value, string name)
		{
			if (value != -1)
			{
				return value;
			}
			value = Shader.PropertyToID(name);
			return value;
		}

		private static int _LocalDisplacementMap = -1;

		private static int _LocalNormalMap = -1;

		private static int _LocalDiffuseMap = -1;

		private static int _LocalMapsCoords = -1;

		private static int _LocalMapsCoordsPrevious = -1;

		private static int _UnderwaterMask = -1;

		private static int _UnderwaterMask2 = -1;

		private static int _AdditiveMask = -1;

		private static int _SubtractiveMask = -1;

		private static int _DisplacementsMask = -1;

		private static int _Gbuffer0 = -1;

		private static int _Gbuffer1 = -1;

		private static int _Gbuffer2 = -1;

		private static int _Gbuffer3 = -1;

		private static int _OcclusionMap = -1;

		private static int _OcclusionMap2 = -1;

		private static int _OcclusionMapProjection = -1;

		private static int _FoamParameters = -1;

		private static int _FoamShoreIntensity = -1;

		private static int _FoamIntensity = -1;

		private static int _WaterTileSizeInvSrt = -1;

		private static int _WaterTileSize = -1;

		private static int _WaterTileSizeInv = -1;

		private static int _WaterTileSizeScales = -1;

		private static int _MaxDisplacement = -1;

		private static int _TotalDisplacementMap = -1;

		private static int _DisplacementMask = -1;

		private static int _WaterDepthTexture = -1;

		private static int _UnityMatrixVPInverse = -1;

		private static int _DepthClipMultiplier = -1;

		private static int _RefractTex = -1;

		private static int _WaterlessDepthTexture = -1;

		private static int _WaterShadowmap = -1;

		private static int _Velocity = -1;

		private static int _CameraDepthTexture2 = -1;

		private static int _PlanarReflectionTex = -1;

		private static int _ButterflyTex = -1;

		private static int _ButterflyPass = -1;

		private static int _Offset = -1;

		private static int _MaxDistance = -1;

		private static int _AbsorptionColorPerPixel = -1;

		private static int _SurfaceOffset = -1;

		private static int _WaterId = -1;

		private static int _Cull = -1;

		private static int _Period = -1;

		private static int _BumpMapST = -1;

		private static int _DetailAlbedoMapST = -1;

		private static int _HeightTex = -1;

		private static int _DisplacementTex = -1;

		private static int _HorizontalDisplacementScale = -1;

		private static int _JacobianScale = -1;

		private static int _RenderTime = -1;

		private static int[] _DisplacementDeltaMaps;
	}
}

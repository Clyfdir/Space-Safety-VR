 
// ──────────────────────────────────────────────────────────────────────────────
//  Hash helpers (same for both noises)
// ──────────────────────────────────────────────────────────────────────────────
inline float3 Random3(float3 p)
{
	// One sin()‑hash per component, each with different large constants
	return frac(sin(float3(
		dot(p, float3(127.1, 311.7,  74.7)),
		dot(p, float3(269.5, 183.3, 246.1)),
		dot(p, float3(113.5, 271.9, 124.6)))
	) * 43758.5453);
}

// ──────────────────────────────────────────────────────────────────────────────
//  3‑D Worley (returns float3: F1, F2, F2–F1)
// ──────────────────────────────────────────────────────────────────────────────
inline float3 Worley3D(float3 pos, float3 gridDims,float time)
{
	float3 cellPos  = pos * gridDims;
	float3 index    = floor(cellPos);
	float3 fractPos = frac(cellPos);

	float F1 = 1.0, F2 = 1.0;

	[loop]
	for (int z = -1; z <= 1; ++z)
	{
		[loop] for (int y = -1; y <= 1; ++y)
		{
			[loop] for (int x = -1; x <= 1; ++x)
			{
				float3 neighbor = float3((float)x, (float)y, (float)z);
				float3 feature  = Random3(index + neighbor);
				//feature *= frac( Random3(3)*time);
				float  dist     = length(neighbor + feature - fractPos);

				if (dist < F1) { F2 = F1; F1 = dist; }
				else if (dist < F2) { F2 = dist; }
			}
		}
	}
	return float3(F1, F2, F2 - F1);
}

// ──────────────────────────────────────────────────────────────────────────────
//  3‑D Simplex noise  (Stefan Gustavson’s branch‑free variant ‑trimmed/GLSL→HLSL)
// ──────────────────────────────────────────────────────────────────────────────
inline float Simplex3D(float3 v)
{
	// Skew factors
	const float F3 = 1.0 / 3.0;
	const float G3 = 1.0 / 6.0;

	// Skew into simplex grid, find base corner
	float3 s  = floor(v + dot(v, float3(F3, F3, F3)));
	float3 x0 = v - s + dot(s, float3(G3, G3, G3));

	// Sort x0 to determine simplex corner ordering
	float3 e  = step(float3(0.0,0.0,0.0), x0.yzx - x0.xyz);
	float3 i1 = e * (1.0 - (x0.yzx < x0.zxy));
	float3 i2 = 1.0 - e * (1.0 - (x0.yzx < x0.zxy));

	// Offsets for other corners
	float3 x1 = x0 - i1 + G3;
	float3 x2 = x0 - i2 + 2.0 * G3;
	float3 x3 = x0 - 1.0 + 3.0 * G3;

	// Permute corner indices
	float4 w;                       // Hash and compute contributions
	w.x = dot(Random3(s), x0);
	w.y = dot(Random3(s + i1), x1);
	w.z = dot(Random3(s + i2), x2);
	w.w = dot(Random3(s + 1.0), x3);

	float4 d = max(0.6 - float4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
	d *= d;  d *= d;

	return dot(d, w) * 32.0;        // Scale to roughly [‑1,1]
}

// ──────────────────────────────────────────────────────────────────────────────
//  Hybrid Worley‑Simplex utility
// ──────────────────────────────────────────────────────────────────────────────
struct HybridParams
{
	float3 worleyGrid;   // Cell count (e.g. 8,8,8)
	float  simplexFreq;  // Simplex scale multiplier
	float  blend;        // 0 = pure Worley (F1), 1 = pure Simplex
	float  ridgeWeight;  // 0‑1 mix of Worley F1 vs ridges (F2‑F1)
};

/*inline float HybridNoise3D(float3 p, HybridParams H)
{
	// Worley part
	float3 w = Worley3D(p, H.worleyGrid);
	float   cell  = 1.0 - w.x;   // invert F1 so cell centers brighter
	float   ridge = w.z;         // F2‑F1 ridge signal
	float   worleyMix = lerp(cell, ridge, H.ridgeWeight);

	// Simplex part
	float simplex = Simplex3D(p * H.simplexFreq) * 0.5 + 0.5; // map to [0,1]

	// Final hybrid
	return lerp(worleyMix, simplex, H.blend);
}*/

float CalculateFbmNoise( float3 xyz,float scale, float3 grid, float amplitude, float time, float octaves, float strength, float frequency )
{
	float3 pos= xyz* scale;
	
	float fbm_Noise =0;
	
	for (int i = 0; i < int(octaves); i++)
	{
		fbm_Noise += Worley3D(pos,grid, time).y * amplitude;
		amplitude *= strength;
		pos *= frequency;
	}
	return fbm_Noise;
}

void TestHybridNoise_float( float3 xyz,float scale, float3 grid, float time , out float h_Noise)
{

    #ifndef SHADERGRAPH_PREVIEW
	/*HybridParams H;
	H.worleyGrid;
	H.simplexFreq;
	H.blend;
	H.ridgeWeight;*/
	//h_Noise = Worley3D(xyz*scale,grid,time);
	h_Noise = CalculateFbmNoise(xyz,1,grid,0.5,time,3,0.5,1.6);
    #else
	h_Noise = CalculateFbmNoise(xyz,1,grid,0.5,time,3,0.5,1.6);
    #endif
		
}
 

     
void CalculateFbm3DNoise_float( float3 noise,float scale, float amplitude , float strength, float frequency, out float fbm_Noise)
{

    #ifndef SHADERGRAPH_PREVIEW
	float3 pos= noise* scale;
	
	fbm_Noise =0;
	
		fbm_Noise += pos.x * amplitude;
		amplitude *= strength;
		pos *= frequency;
	
		fbm_Noise += pos.y * amplitude;
		amplitude *= strength;
		pos *= frequency;
	
		fbm_Noise += pos.z * amplitude;
	
	
	
    #else
	float3 pos= noise* scale;
	
	fbm_Noise =0;
	
	fbm_Noise += pos.x * amplitude;
	amplitude *= strength;
	pos *= frequency;
	
	fbm_Noise += pos.y * amplitude;
	amplitude *= strength;
	pos *= frequency;
	
	fbm_Noise += pos.z * amplitude;
	
	
    #endif
		
}
 
void CalculateSineCurve_float( float x, float strechY, float strechX, float moveX, float moveY, out float y)
{

    #ifndef SHADERGRAPH_PREVIEW

    y = strechY*sin(strechX*(x+moveX))+moveY;
	
    #else
    y = 1;
    #endif
		
}

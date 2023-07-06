void TheftSpiral_float(float t, float len, float2 uv, out float v1, out float v2, out float3 p)
{
	v1=0., v2=0.;
    	
    for (int i = 0; i < 100; i++) {
    	p = .035*float(i) *  float3(uv, 1.);
    	p += float3(.22,  .3,  -1.5 -sin(t*1.3)*.1);
    	
    	for (int i = 0; i < 8; i++)                // IFS
    		p = abs(p) / dot(p,p) - 0.659;

        const float p2 = dot(p,p)*.0015;
    	v1 += p2 * ( 1.8 + sin(len*13.0  +.5 -t*2.) );
    	v2 += p2 * ( 1.5 + sin(len*13.5 +2.2 -t*3.) );
    }
}





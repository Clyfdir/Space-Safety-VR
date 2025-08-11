#ifndef GET_OBJECT_FORWARD_INCLUDED
#define GET_OBJECT_FORWARD_INCLUDED

// Custom function that returns the object's forward vector in world space
void GetObjectForward_float(float3 UserVecWS,out float3 Result)
{
    // Third column of unity_ObjectToWorld is the forward vector in world space
    float3 ForwardWS = normalize(float3(
        unity_ObjectToWorld._m02,
        unity_ObjectToWorld._m12,
        unity_ObjectToWorld._m22
    ));
    float rawDot = dot(ForwardWS, normalize(UserVecWS));
     Result = saturate((rawDot + 1.0) * 0.5); // Remap
}

#endif

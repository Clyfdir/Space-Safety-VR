void ComputeLutUV_float(float3 color, float size, out float2 uv, out float frac)
{
    float slice = color.b * (size -1);
    float sliceFloor = floor(slice);
    frac = slice - sliceFloor;

    float width  = size * size;

    uv.x = (color.r*size + sliceFloor * size) / width;
    uv.y = color.g;
}

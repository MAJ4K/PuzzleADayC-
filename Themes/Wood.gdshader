shader_type canvas_item;

uniform float ASPECT;
uniform float SCALE;

float random (float x) {
    return fract(sin(x * 12.9898) * 43758.5453123);
}

float RNGF(in vec2 st) {
	return fract(sin(dot(st.xy,
		vec2(12.9898,78.233)))
		* 43758.5453123);
}
vec2 RNGV2(vec2 p){
	vec3 a = fract(p.xyx * vec3(111.11,333.33,444.44));
	a += dot(a,a+33.51);
	return fract(vec2(a.x * a.y, a.y * a.z));
}

void vertex() {}

float lines(vec2 pos, float b){
	float scale = 1.0;
	pos *= scale;
	return smoothstep(0.0,
		.5+b*.5,
		abs((sin(pos.x*3.1415)+b*2.0))*.5);
}


// Value noise by Inigo Quilez - iq/2013
// https://www.shadertoy.com/view/lsf3WH
float noise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);
    vec2 u = f*f*(3.0-2.0*f);
    return mix( mix( RNGF( i + vec2(0.0,0.0) ),
                     RNGF( i + vec2(1.0,0.0) ), u.x),
                mix( RNGF( i + vec2(0.0,1.0) ),
                     RNGF( i + vec2(1.0,1.0) ), u.x), u.y);
}

float fracNoise(vec2 uv,int octs){
	float noi = 0.0;
	float l = 0.5;
	mat2 m = mat2(vec2(1.6,1.2),vec2(-1.2,1.6));
	for (int i = 0; i < octs; i++){
		noi += l*noise(uv); uv *= m;
		l /= 2.0;
	}
	return noi;
}

mat2 rotate2d(float angle){
	return mat2(vec2(cos(angle),-sin(angle)),
				vec2(sin(angle),cos(angle)));
}

float noise1D(float p){
	float fl = floor(p);
	float fc = fract(p);
	return mix(random(fl), random(fl + 1.0), fc);
}

void fragment() {
	vec2 uv = ((UV) * vec2(ASPECT,1.0) * SCALE);
	vec2 fuv = fract(uv);
	vec2 iuv = floor(uv);
	vec2 test = vec2(0.0);
	
	uv *= vec2(3.0,.333);
	vec2 pos = rotate2d(noise(uv.yx)) * uv.yx;
	test.x = lines(vec2(pos.x,noise1D(pos.y * 5.0)).yx,0.01);
	
	COLOR = vec4(1.0,1.0,1.0,test.x);
}

void light() {}

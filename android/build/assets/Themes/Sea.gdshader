shader_type canvas_item;

uniform float ASPECT;
uniform float SCALE;


mat2 rotate2d(float angle){
	return mat2(vec2(cos(angle),-sin(angle)),
				vec2(sin(angle),cos(angle)));
}

float random (in vec2 st) {
    return fract(sin(dot(st.xy,
                         vec2(12.9898,78.233)))
                * 43758.5453123);
}

vec2 RNGV2(vec2 p){
	vec3 a = fract(p.xyx * vec3(111.11,333.33,444.44));
	a += dot(a,a+33.51);
	return fract(vec2(a.x * a.y, a.y * a.z));
}
vec3 RNGV3(vec2 co){
	vec3 a = fract( cos( co.x*8.3e-3 + co.y )*vec3(1.3e5, 4.7e5, 2.9e5) );
	vec3 b = fract( sin( co.x*0.3e-3 + co.y )*vec3(8.1e5, 1.0e5, 0.1e5) );
	vec3 c = mix(a, b, 0.5);
	return c;
}

void vertex() {}
float field(in vec3 p,float s) {
	float strength = 7. + .03 * log(1.e-6 + fract(sin(TIME) * 4373.11));
	float accum = s/4.;
	float prev = 0.;
	float tw = 0.;
	for (int i = 0; i < 26; ++i) {
		float mag = dot(p, p);
		p = abs(p) / mag + vec3(-.5, -.4, -1.5);
		float w = exp(-float(i) / 7.);
		accum += w * exp(-strength * pow(abs(mag - prev), 2.2));
		tw += w;
		prev = mag;
	}
	return max(0., 5. * accum / tw - .7);
}

// Less iterations for second layer
float field2(in vec3 p, float s) {
	float strength = 7. + .03 * log(1.e-6 + fract(sin(TIME) * 4373.11));
	float accum = s/4.;
	float prev = 0.;
	float tw = 0.;
	for (int i = 0; i < 18; ++i) {
		float mag = dot(p, p);
		p = abs(p) / mag + vec3(-.5, -.4, -1.5);
		float w = exp(-float(i) / 7.);
		accum += w * exp(-strength * pow(abs(mag - prev), 2.2));
		tw += w;
		prev = mag;
	}
	return max(0., 5. * accum / tw - .7);
}

float lines(in vec2 pos, float b){
    float scale = 10.0;
    pos *= scale;
    return smoothstep(0.0,
                    .5+b*.5,
                    abs((sin(pos.x*3.1415)+b*2.0))*.5);
}

vec4 mainImage(vec2 uv){
	const vec2 vel = vec2(1., 0.);
	const float tiles = 2.;
	const float pi = 3.14159;
	const vec3 color = vec3(0.3, 0.5, 0.7);
    //wave
    vec2 wave = uv;
    wave.x += sin(uv.y*5.+TIME)*0.1;
    wave.y += cos(uv.x*5.+TIME)*0.1;
    uv += wave;
    
    uv+=vel*TIME;
    
    vec2 index = floor(tiles * uv)/tiles;
    float t = floor(random(index)*4.)/4.;
    
    uv = 2.0 * fract(tiles * uv) - 1.0;
    uv *= rotate2d(t*pi*2.);
    
    float c = step(uv.x, uv.y)*0.9;
    c=abs(sin(TIME*fract((random(index+c)+0.1))));
	return vec4((c*.5+0.5) * color,1.0);
}

void fragment() {
	vec2 uv = ((UV) * vec2(ASPECT,1.0) * SCALE);

	COLOR = mainImage(-uv.yx);
	
}

void light() {}

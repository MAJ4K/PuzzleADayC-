shader_type canvas_item;

uniform vec2 RESOLUTION;
uniform float ASPECT;
uniform float SCALE;

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

vec4 mainImage(vec2 uv)
{
	float t = TIME * .1 + ((.25 + .05 * sin(TIME * .1))/(length(uv.xy) + .07)) * 2.2;
	float si = sin(t);
	float co = cos(t);
	mat2 ma = mat2(vec2(co,si),vec2(-si,co));

	float v1, v2, v3;
	v1 = v2 = v3 = 0.0;
	
	float s = 0.0;
	for (int i = 0; i < 90; i++)
	{
		vec3 p = s * vec3(uv, 0.0);
		p.xy *= ma;
		p += vec3(.22, .3, s - 1.5 - sin(TIME * .13) * .1);
		for (int j = 0; j < 8; j++)	p = abs(p) / dot(p,p) - 0.659;
		v1 += dot(p,p) * .0015 * (1.8 + sin(length(uv.xy * 13.0) + .5  - TIME * .2));
		v2 += dot(p,p) * .0013 * (1.5 + sin(length(uv.xy * 14.5) + 1.2 - TIME * .3));
		v3 += length(p.xy*10.) * .0003;
		s  += .035;
	}
	
	float len = length(uv);
	v1 *= smoothstep(.7, .0, len);
	v2 *= smoothstep(.5, .0, len);
	v3 *= smoothstep(.9, .0, len);
	
	vec3 col = vec3( v3 * (1.5 + sin(TIME * .2) * .4),
					(v1 + v3) * .3,
					 v2) + smoothstep(0.2, .0, len) * .85 + smoothstep(.0, .6, v3) * .3;

	return vec4(min(pow(abs(col), vec3(1.2)), 1.0), 1.0);
}

vec4 mainImage2(vec2 uv) {
	vec2 uvs = uv; //RESOLUTION.xy / max(RESOLUTION.x, RESOLUTION.y);
	vec3 p = vec3(uvs / 4., 0) + vec3(1., -1.3, 0.);
	p += .2 * vec3(sin(TIME / 16.), sin(TIME / 12.),  sin(TIME / 128.));
	
	float freqs[4];
	//Sound
	freqs[0] = 0.2;//texture( iChannel0, vec2( 0.01, 0.25 ) ).x;
	freqs[1] = 0.3;//texture( iChannel0, vec2( 0.07, 0.25 ) ).x;
	freqs[2] = 0.5;//texture( iChannel0, vec2( 0.15, 0.25 ) ).x;
	freqs[3] = 0.1;//texture( iChannel0, vec2( 0.30, 0.25 ) ).x;

	float t = field(p,freqs[2]);
	float v = (1. - exp((abs(uv.x) - 1.) * 6.)) * (1. - exp((abs(uv.y) - 1.) * 6.));
	
    //Second Layer
	vec3 p2 = vec3(uvs / (4.+sin(TIME*0.11)*0.2+0.2+sin(TIME*0.15)*0.3+0.4), 1.5) + vec3(2., -1.3, -1.);
	p2 += 0.25 * vec3(sin(TIME / 16.), sin(TIME / 12.),  sin(TIME / 128.));
	float t2 = field2(p2,freqs[3]);
	vec4 c2 = mix(.4, 1., v) * vec4(1.3 * t2 * t2 * t2 ,1.8  * t2 * t2 , t2* freqs[0], t2);
	
	
	//Let's add some stars
	//Thanks to http://glsl.heroku.com/e#6904.0
	vec2 seed = p.xy * 2.0;	
	seed = floor(seed * RESOLUTION.x);
	vec3 rnd = RNGV3( seed );
	vec4 starcolor = vec4(pow(rnd.y,40.0));
	
	//Second Layer
	vec2 seed2 = p2.xy * 2.0;
	seed2 = floor(seed2 * RESOLUTION.x);
	vec3 rnd2 = RNGV3( seed2 );
	starcolor += vec4(pow(rnd2.y,40.0));
	
	return mix(freqs[3]-.3, 1., v) * vec4(1.5*freqs[2] * t * t* t , 1.2*freqs[1] * t * t, freqs[3]*t, 1.0)+c2+starcolor;
}

void fragment() {
	vec2 uv = ((UV - 0.5) * vec2(ASPECT,1.0) * SCALE);
	vec2 fuv = fract(uv);
	vec2 iuv = floor(uv);
	float noi = 0.0;


	COLOR = mainImage2(uv);
	
}

void light() {}

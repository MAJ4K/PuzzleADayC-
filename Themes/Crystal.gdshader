shader_type canvas_item;

uniform float ASPECT;
uniform float SCALE;

float RNGF(vec2 st) {
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
	return mix(a,b,0.5);
}

void vertex() {}

//void mainImage(){
//	vec2 uv = fragCoord/iResolution.y + vec2(-.2, .05);//*iTime;
//
//    // Obtain the height map (rounded Voronoi border) value, then another nearby. 
//    vec2 c = hMap(uv);
//    vec2 c2 = hMap(uv + .004);
//
//    // Take a factored difference of the values above to obtain a very, very basic gradient value.
//    // Ie. - a measurement of the bumpiness, or bump value.
//    float b = max(c2.x - c.x, 0.)*16.;
//
//    // Use the height map value to produce some color. It's all made up on the spot, so don't pay it
//    // too much attention.
//    //
//    vec3 col = vec3(1, .05, .25)*c.x; // Red base.
//    float sv = Voronoi(uv*16. + c.y)*.66 + (1.-Voronoi(uv*48. + c.y*2.))*.34; // Finer overlay pattern.
//    col = col*.85 + vec3(1, .7, .5)*sv*sqrt(sv)*.3; // Mix in a little of the overlay.
//    col += (1. - col)*(1.-smoothstep(0., fwidth(c.y)*3., c.y - .22))*c.x; // Highlighting the border.
//    col *= col; // Ramping up the contrast, simply because the deeper color seems to look better.
//
//    // Taking a pattern sample a little off to the right, ramping it up, then combining a bit of it
//    // with the color above. The result is the flecks of yellowy orange that you see. There's no physics
//    // behind it, but the offset tricks your eyes into believing something's happening. :)
//    sv = col.x*Voronoi(uv*6. + .5);
//    col += vec3(.7, 1, .3)*pow(sv, 4.)*8.;
//
//    // Apply the bump - or a powered variation of it - to the color for a bit of highlighting.
//    col += vec3(.5, .7, 1)*(b*b*.5 + b*b*b*b*.5);
//
//
//    // Basic gamma correction
//	fragColor = vec4(sqrt(clamp(col, 0., 1.)), 1);
//
//}

vec4 VoronoiRounded(vec2 uv,float seed){
	vec2 iuv = floor(uv);
	vec2 fuv = fract(uv);

	vec3 d = vec3(1.3); // 1.4, etc.
	float mindist = 100.0;
	vec2 id;
    
	for(float y = -2.0; y <= 2.0; y++)
	for(float x = -2.0; x <= 2.0; x++){
		vec2 offs = vec2(x, y);
		vec2 n = RNGV2(iuv + offs);
		vec2 p = offs + sin(n * seed) * 0.35;
        
		float r = dot(fuv - p,fuv - p);

		// 1st, 2nd and 3rd nearest squared distances.
		d.z = max(d.x, max(d.y, min(d.z, r))); // 3rd.
		d.y = max(d.x, min(d.y, r)); // 2nd.
		d.x = min(d.x, r); // Closest.
		if(length(fuv - p) < mindist){
			mindist = length(fuv - p);
			id = iuv + offs;
		}
	}
    
	d = sqrt(d); // Squared distance to distance.
    
    // Fabrice's formula.
//	return min(2./(1./max(d.y - d.x, .001) + 1./max(d.z - d.x, .001)), 1.);
	return vec4(
		id.x,id.y,
		min(1.5/(0.50/max(d.y - d.x, .001) + 1./max(d.z - d.x, 0.001)), 1.),
		1.0
	);
}

vec4 Voronoi(vec2 uv,float seed){
	vec2 fuv = fract(uv);
	vec2 iuv = floor(uv);
	
	float minDist = 100.0;
	vec2 id = vec2(0.0);
	vec3 k = vec3(1.0);
	
	for(float y = -1.0; y <= 1.0; y++)
	for(float x = -1.0; x <= 1.0; x++){
		vec2 offs = vec2(x,y);
		vec2 n = RNGV2(iuv + offs);
		vec2 p = offs + sin(n * seed) * 0.3;
		float d = length(fuv - p);
		
		if (d < minDist){
			minDist = d;
			id = offs + iuv;
		}
	}
	return vec4(id.x,id.y,minDist,1.0);
}

mat2 rotate2d(float angle){
	return mat2(vec2(cos(angle),-sin(angle)),
				vec2(sin(angle),cos(angle)));
}

void fragment() {
	vec2 uv = ((UV) * vec2(ASPECT,1.0) * SCALE);
	vec2 fuv = fract(uv);
	vec2 iuv = floor(uv);
	float noi = 0.0;

	vec4 vor = VoronoiRounded(uv,121.233);
	vec3 colA = vec3(0.6,0.6,0.2) + mix(0.1,0.4,RNGF(vor.yx));
	vec3 colB = vec3(0.3,0.6,0.3) + mix(0.2,0.5,RNGF(vor.xy));
	float t = sin((vor.x + vor.y)/8.0 + TIME*0.4);
	
	COLOR.rgb = mix(colB,colA,t) * mix(0.1,1.0,vor.z) ;
}

void light() {}

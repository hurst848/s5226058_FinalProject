//~ fu-4, fu-5, fu-6

uniform vec3 u_LightColour;
uniform vec3 u_LightPosition;
uniform float u_LightAmbientStrength;

varying vec3 v_Normal;
varying vec3 v_FragmentPosition;

void main()
{
	vec3 ambientLight = u_LightAmbientStrength * u_LightColour;
	
	vec3 norm = normalize(v_Normal);
	vec3 lightDirection = normalize(u_LightPosition - v_FragmentPosition);
	float diff = max(dot(norm, u_LightColour), 0.0);
	vec3 diffuseLight = diff * u_LightColour;

	vec3 light = ambientLight + diffuseLight;

	gl_FragColor = vec4(light, 1.0) * vec4(1,0,0,1);

}
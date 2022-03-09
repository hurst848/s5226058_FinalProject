uniform mat4 u_ModelMatrix;
uniform mat4 u_ProjectionMatrix;
uniform mat4 u_ViewMatrix;

attribute vec3 a_Position;
attribute vec2 a_TextureCoordinates;
attribute vec3 a_Normal;

varying vec2 v_TexCoords;
varying vec3 v_Normal;
varying vec3 v_FragmentPosition;

void Main()
{
	v_FragmentPosition = vec3(u_ModelMatrix * vec4(a_Position, 1.0));
	gl_Position = u_ProjectionMatrix * u_ViewMatrix * u_ModelMatrix * vec4(a_Position, 1.0);
	v_TexCoords = a_TextureCoordinates;
	v_Normal = vec3(u_ModelMatrix * vec4(a_Normal, 0));
}
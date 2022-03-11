#include "Engine/HGE.h"
#include "ProcedualPlanetGenerator/HGE_PPG.h"

#include "CameraController.h"

#include <memory>
#include <string>
#include <vector>

using namespace HGE;



int main()
{

	std::vector<vec3> points = 
	{
		vec3(-0.5f,0.5f,0.0f),
		vec3(0.5f,0.5f,0.0f),
		vec3(-0.5f,-0.5f,0.0f),
		vec3(0.5f,-0.5f,0.0f)
	};
	std::vector<int> indicies =
	{
		0, 2, 3,
		0, 3, 1
	};
	std::vector<vec3> normals =
	{
		vec3(0,0,1),vec3(0,0,1),
		vec3(0,0,1),vec3(0,0,1),
		vec3(0,0,1),vec3(0,0,1)
	};

	


	// Create Core
	std::shared_ptr<Core> core = Core().Initialize();

	// Load Resources

	std::shared_ptr<Shader> vertex = core->Resources->AddResource<Shader>("../shaders/lightShader.vert");
	std::shared_ptr<Shader> fragment = core->Resources->AddResource<Shader>("../shaders/lightShader.frag");
	
	std::shared_ptr<Shader> tstvertex = core->Resources->AddResource<Shader>("../shaders/BasicCubeSphere.vert");
	std::shared_ptr<Shader> tstfragment = core->Resources->AddResource<Shader>("../shaders/BasicCubeSphere.frag");

	std::shared_ptr<HGE::Mesh> ballMesh = core->Resources->AddResource<HGE::Mesh>("../meshes/sphere.obj");
	std::shared_ptr<Texture> ballTexture = core->Resources->AddResource<Texture>("../textures/sphereTexture.png");

	MeshBuilder tstbuilder;
	std::shared_ptr<HGE::Mesh> testSquare = tstbuilder.CreateMesh("testSquare", points, normals, indicies);
	
	// Create Camera
	std::weak_ptr<Entity> cam = core->AddEntity();
		std::weak_ptr<Camera> cameraComponent = cam.lock()->AddComponent<Camera>();
			cameraComponent.lock()->SetAsMainCamera();
			cameraComponent.lock()->ViewDistance = 10000;
		std::weak_ptr<CameraController> cmsontoller = cam.lock()->AddComponent<CameraController>();
			
	// Create and build a test Chunk
	
	core->StartEngine();
	
	
	return 0;
}



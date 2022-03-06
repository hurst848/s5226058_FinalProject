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

	std::shared_ptr<Mesh> ballMesh = core->Resources->AddResource<Mesh>("../meshes/sphere.obj");
	std::shared_ptr<Texture> ballTexture = core->Resources->AddResource<Texture>("../textures/sphereTexture.png");

	MeshBuilder tstbuilder;
	std::shared_ptr<Mesh> testSquare = tstbuilder.CreateMesh("testSquare", points, normals, indicies);
	
	// Create Camera
	std::weak_ptr<Entity> cam = core->AddEntity();
		std::weak_ptr<Camera> cameraComponent = cam.lock()->AddComponent<Camera>();
			cameraComponent.lock()->SetAsMainCamera();
			cameraComponent.lock()->ViewDistance = 10000;
		std::weak_ptr<CameraController> cmsontoller = cam.lock()->AddComponent<CameraController>();
			

	std::weak_ptr<Entity> tstSquare = core->AddEntity();
		std::weak_ptr<MeshRenderer> tstRenderer = tstSquare.lock()->AddComponent<MeshRenderer>();
			std::shared_ptr<ShaderProgram> tstProgram = std::make_shared<ShaderProgram>();
				tstProgram->AddShader(tstvertex);
				tstProgram->AddShader(tstfragment);
				tstProgram->Compile();
			tstRenderer.lock()->SetMesh(testSquare);
			tstRenderer.lock()->SetShader(tstProgram);
			tstRenderer.lock()->Render = true;


	// create planet
	std::weak_ptr<Entity> planet = core->AddEntity();
		std::weak_ptr<Sphere> mesh = planet.lock()->AddComponent<Sphere>();
			mesh.lock()->GenerateNormalizedCube(10, 5);
		planet.lock()->GetTransform()->SetPosition(0,0,0);



	std::shared_ptr<Sphere> s = std::make_shared<Sphere>();
	//s->GenerateFibonacciSphere(1000, 100);

	//for (int i = 0; i < s->points.size(); i++)
	//{
	//	// Create Ball
	//	std::weak_ptr<Entity> ball = core->AddEntity();
	//	std::weak_ptr<MeshRenderer> ballRenderer = ball.lock()->AddComponent<MeshRenderer>();
	//	std::shared_ptr<ShaderProgram> ballProgram = std::make_shared<ShaderProgram>();
	//	ballProgram->AddShader(vertex);
	//	ballProgram->AddShader(fragment);
	//	ballProgram->Compile();
	//	ballRenderer.lock()->SetMesh(ballMesh);
	//	ballRenderer.lock()->SetTexture(ballTexture);
	//	ballRenderer.lock()->SetShader(ballProgram);
	//	ballRenderer.lock()->Render = true;
	//	ball.lock()->GetTransform()->SetPosition(s->points.at(i));
	//	ball.lock()->GetTransform()->SetScale(0.01f, 0.01f, 0.01f);
	//}
	//
	
	// Start the Engine

	std::shared_ptr<Chunk> testChunk = Chunk().Initialize();
	//Chunk tstChnk;


	core->StartEngine();
	
	
	return 0;
}



#include "Engine/HGE.h"
#include "ProcedualPlanetGenerator/HGE_PPG.h"

#include "PlayerController.h"
#include "OponentController.h"
#include "BallController.h"
#include "CameraController.h"

#include <memory>
#include <string>
#include <vector>

using namespace HGE;



int main()
{
	// Create Core
	std::shared_ptr<Core> core = Core().Initialize();

	// Load Resources

	std::shared_ptr<Shader> vertex = core->Resources->AddResource<Shader>("../shaders/lightShader.vert");
	std::shared_ptr<Shader> fragment = core->Resources->AddResource<Shader>("../shaders/lightShader.frag");

	std::shared_ptr<Mesh> ballMesh = core->Resources->AddResource<Mesh>("../meshes/sphere.obj");
	std::shared_ptr<Texture> ballTexture = core->Resources->AddResource<Texture>("../textures/sphereTexture.png");

	
	// Create Camera
	std::weak_ptr<Entity> cam = core->AddEntity();
		std::weak_ptr<Camera> cameraComponent = cam.lock()->AddComponent<Camera>();
			cameraComponent.lock()->SetAsMainCamera();
			cameraComponent.lock()->ViewDistance = 10000;
		std::weak_ptr<AudioListener> listener = cam.lock()->AddComponent<AudioListener>();
			listener.lock()->SetActiveListner();
		std::weak_ptr<AudioListener> camListener = cam.lock()->AddComponent<AudioListener>();
			camListener.lock()->SetSelf(camListener.lock());
			camListener.lock()->SetActiveListner();
		std::weak_ptr<CameraController> cmsontoller = cam.lock()->AddComponent<CameraController>();
			
	
	std::shared_ptr<Sphere> s = std::make_shared<Sphere>();
	s->GenerateFibonacciSphere(1000, 100);

	for (int i = 0; i < s->points.size(); i++)
	{
		// Create Ball
		std::weak_ptr<Entity> ball = core->AddEntity();
		std::weak_ptr<MeshRenderer> ballRenderer = ball.lock()->AddComponent<MeshRenderer>();
		std::shared_ptr<ShaderProgram> ballProgram = std::make_shared<ShaderProgram>();
		ballProgram->AddShader(vertex);
		ballProgram->AddShader(fragment);
		ballProgram->Compile();
		ballRenderer.lock()->SetMesh(ballMesh);
		ballRenderer.lock()->SetTexture(ballTexture);
		ballRenderer.lock()->SetShader(ballProgram);
		ballRenderer.lock()->Render = true;
		ball.lock()->GetTransform()->SetPosition(s->points.at(i));
		ball.lock()->GetTransform()->SetScale(0.01f, 0.01f, 0.01f);
	}
	
	
	// Start the Engine
	core->StartEngine();
	
	
	return 0;
}



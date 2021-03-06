cmake_minimum_required(VERSION 3.0)
project(HURST_ENGINE_PROCEDURAL_PLANET_GENERATION)

find_package(OpenVDB CONFIG REQUIRED)
find_package(Boost 1.77.0) 
set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS ON)
include_directories(
	src
	contrib
)

if (MSVC)
	include_directories(
		contrib/msvc_windows_x86/SDL/include
		contrib/msvc_windows_x86/Glew/include
	)
	link_directories(
		${CMAKE_SOURCE_DIR}/contrib/msvc_windows_x86/SDL/lib
		${CMAKE_SOURCE_DIR}/contrib/msvc_windows_x86/Glew/lib
	)
else()
endif()

if (MSVC)
	add_definitions(-DSDL_MAIN_HANDLED)
endif()

add_library(engine
			src/Engine/Core.h
			src/Engine/Core.cpp
			src/Engine/Entity.h
			src/Engine/Entity.cpp
			src/Engine/Component.h
			src/Engine/Component.cpp
			src/Engine/Resource.h
			src/Engine/Environment.h
			src/Engine/Environment.cpp
			src/Engine/Screen.h
			src/Engine/Screen.cpp
			src/Engine/Transform.h
			src/Engine/Transform.cpp
			src/Engine/Maths.h
			src/Engine/MeshRenderer.h
			src/Engine/MeshRenderer.cpp
			src/Engine/Mesh.h
			src/Engine/Mesh.cpp
			src/Engine/Shader.h
			src/Engine/Shader.cpp
			src/Engine/Texture.h
			src/Engine/Texture.cpp
			src/Engine/Assets.h
			src/Engine/Assets.cpp
			src/Engine/Input.h
			src/Engine/Input.cpp
			src/Engine/Camera.h
			src/Engine/Camera.cpp
			src/Engine/MeshBuilder.h
			src/Engine/MeshBuilder.cpp

			src/Engine/PhysicsEngine.h
			src/Engine/PhysicsEngine.cpp
			src/Engine/Collider.h
			src/Engine/Collider.cpp
			src/Engine/BoxCollider.h
			src/Engine/BoxCollider.cpp
			src/Engine/SphereCollider.h
			src/Engine/SphereCollider.cpp
			src/Engine/Rigidbody.h
			src/Engine/Rigidbody.cpp

			src/Engine/HGE.h



			src/ProcedualPlanetGenerator/Sphere.h
			src/ProcedualPlanetGenerator/Sphere.cpp
			src/ProcedualPlanetGenerator/Chunk.h
			src/ProcedualPlanetGenerator/Chunk.cpp
			src/ProcedualPlanetGenerator/MarchingCubes.h
			src/ProcedualPlanetGenerator/MarchingCubes.cpp
			src/ProcedualPlanetGenerator/Face.h
			src/ProcedualPlanetGenerator/Face.cpp
			src/ProcedualPlanetGenerator/TerrainRenderer.h
			src/ProcedualPlanetGenerator/TerrainRenderer.cpp

			src/ProcedualPlanetGenerator/HGE_PPG.h
			)

add_executable(testGame
    		src/Game/Application.cpp
			src/Game/CameraController.h
			src/Game/CameraController.cpp
			)


if (Boost_FOUND)
		include_directories(${Boost_INCLUDE_DIRS})
		target_link_libraries(engine 
			SDL2
			glew32
			opengl32
			OpenVDB::openvdb
			)
		target_link_libraries(testGame engine)
		
else()
		target_link_libraries(engine 
			SDL2
			glew32
			opengl32
			)
		target_link_libraries(testGame engine)
		
endif()










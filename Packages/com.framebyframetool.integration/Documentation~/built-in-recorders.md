# Built-in Recorders

The package includes recorder components for common Unity debugging data.

`[IMAGE PLACEHOLDER: Inspector showing several Frame by Frame recorder components on a GameObject.]`

## RecordPhysics

Records:

- `SphereCollider`
- `BoxCollider`
- `CapsuleCollider`
- collision enter events
- collision contact points
- Rigidbody velocity

Related options:

- `Colliders`
- `Collisions`
- `Physics`

Use this on objects where physics data matters. Avoid adding it to every physics object in large scenes unless you also filter by layer, distance, or gameplay relevance.

## RecordAI

Records `NavMeshAgent` state and path data.

Related option:

- `Navigation`

Recorded data includes:

- agent enabled state
- path status
- path corners
- path segment debug lines

## RecordNavMesh

Records the calculated NavMesh triangulation.

Related option:

- `NavMesh`

This can be useful when debugging baked navigation geometry. It can also be expensive, so enable it only when needed.

## RecordAnimations

Records Animator state.

Related option:

- `Animations`

Recorded data includes:

- Animator enabled state
- speed
- root motion status
- layer state
- clip weights
- normalized time
- Animator parameters

## RecordLog

Records Unity log messages.

Related option:

- `Log`

Errors, exceptions, and asserts can be recorded as Frame by Frame events with stack traces.

## RecordPlane

Records a helper plane in the scene.

Related option:

- `ShapeHelpers`

## RecordScreenshot

Records screen captures as textured helper planes.

Related option:

- `ShapeHelpers`

Use this carefully. Screenshots are larger than scalar properties and shapes.

# CityForge 🏙️

A procedural 3D city construction tool for game developers (indie and AAA).  
Build complete cities with roads, zones, and buildings — then export directly into your game engine.

> ⚠️ Early development — features are being added incrementally.

---

## v0.2 — Snap & Junctions

Road network now has proper node-based architecture with snapping and junction geometry.

### What's new
- Node-based road network (`RoadNetwork`, `RoadNode`, `RoadEdge`)
- Snap to existing road endpoints with visual indicator
- Automatic junction discs fill gaps between connected roads
- Edges trim correctly at junctions — no overlaps or gaps
- Junctions rebuild dynamically when new roads connect

### Demo

**Snap + junctions**  
![Snap and junctions](gifs/v0.2-snap-junctions.gif)

---

## v0.1 — Road System

First working version of the road placement system.

### Features
- Straight and curved road drawing
- Bézier-based curve generation
- Procedural mesh generation along the road path
- RTS-style camera (WASD + scroll zoom + middle-click rotation)
- Toolbar UI to switch between road modes

### Demo

**Camera controls**  
![Camera](gifs/SimpleCameraSystem.gif)

**Straight roads**  
![Straight roads](gifs/StraightRoads.gif)

**Curved roads**  
![Curved roads](gifs/CurveRoads.gif)

---

## Roadmap
- [x] Road placement (straight + curved)
- [x] RTS camera
- [x] Snap to existing nodes
- [ ] Road intersections
- [ ] Lot detection
- [ ] Lot subdivision
- [ ] Zoning system
- [ ] Procedural buildings

---

## Built with
- Unity 6 (6000.3.2f1)
- Unity Splines package

## License
MIT

# Contact Shadows and Auto Exposure

Both effects are disabled by default in Volume profiles. They use compute shaders
and support ordinary 2D cameras. XR, child rendering contexts, and hardware dynamic
resolution are excluded from this first implementation. Render Scale is supported.

## Contact Shadows

Add **Lighting > Contact Shadows** to a Volume profile, override **Enable**, and
turn it on. The renderer produces a short screen-space shadow for the main
directional light. No extra graph node or camera Post Processing toggle is needed;
the graph must use the standard Render Driven and object-lighting passes.

`ContactShadowsPass` uses the renderer's `InjectPass` mechanism after the main
context's `RenderDrivenPass`, once complete depth and any required depth resolve
are available. Frame preparation snapshots the Volume settings and main light,
declares the depth requirements, and resets contact-shadow globals for every
camera. The injected pass only generates the texture; the lighting passes read
it through explicit RenderGraph dependencies. There is no late post-process
fallback when the graph has no active main-context Render Driven node.

The effect applies to opaque receivers in Forward, Stencil Deferred, and Clustered
Deferred. Lit Shader Graphs and built-in materials that use the shared fragment
lighting functions inherit support. Custom lighting and vertex-only lighting need
their own integration. Transparent receivers are excluded.

Useful starting values are Length **0.15**, Sample Count **16**, Thickness **0.02**,
and Bias **0.002**, with world units interpreted using the scene scale. Opacity
controls strength. Max Distance and Fade Distance limit evaluation to nearby
geometry. Distance Scale increases the ray length with view depth; leave it at
zero for a fixed world-space length.

The renderer requests complete opaque depth before lighting, including when the
graph mixes forward and deferred objects. This can add a full depth prepass and
depth resolve. Stencil Deferred uses texture sampling instead of framebuffer fetch
while this effect is active. The contact texture is full resolution, R16F, with no
separate temporal history. TAA can smooth its changing sample pattern.

Contact shadows only see geometry represented in camera depth. They complement
shadow maps: hidden/offscreen geometry and very thin details can be missed.
Increasing thickness may hide gaps but can introduce halos. The shader skips the
origin depth texel to reduce self-shadowing. Direct main-light visibility is
combined with the shadow map; GI, emission, and other lights are not darkened.

When a candidate hit lies on a reliably reconstructed receiver plane, the shader
rejects it while the ray remains on the visible side of that plane. This prevents
point-sampled depth from making an unobstructed slope shadow itself. Plane
reconstruction uses eight neighboring depth samples only on the first candidate
hit; discontinuities and unreliable estimates retain the original depth test.
Curved surfaces can still show sampling noise. If that noise is distracting,
disable Contact Shadows and retain the main directional-light shadow map.

The 2026-10-29 D3D11 GPU regression passed 21 cases: unobstructed perspective and
orthographic slopes, thin foreground occluders, depth steps, and backlit planes.
The reproduced sloped-plane false shadows fell from 1776 pixels to zero; the
tested real occluders and backlit shadows remained pixel-identical. Curved-surface
noise and other graphics APIs are outside that validation.

## Histogram Auto Exposure

Add **Post-processing > Auto Exposure**, override **Enabled**, and turn it on.
Enable Post Processing on the camera and use the graph's existing Post Process
node. Its automatically registered Auto Exposure entry starts enabled; the Volume
controls whether work is scheduled.

In camera stacks, enable this effect on the Base camera. Overlay cameras skip
automatic exposure to avoid metering an already exposed/tonemapped Base image.
UI drawn by an Overlay therefore does not alter the Base camera's metering.

| Setting | Meaning | Default |
| --- | --- | --- |
| Low / High Percent | Discard the darkest/brightest portions of the histogram | 10 / 90 |
| Min / Max EV | Clamp metered EV100; limit brightening in darkness and darkening in bright scenes | -8 / 16 |
| Compensation | Brightness offset in stops; positive values brighten | 0 |
| Speed Dark To Light | Adaptation rate when entering a brighter scene | 3 |
| Speed Light To Dark | Adaptation rate when entering a darker scene | 1 |

Metering uses a 128-bin histogram over uniformly sampled scene color, with the
longest metering dimension capped at 512 pixels. Percentile clipping removes small
bright highlights or deep shadows from the average. The selected luminance maps
to 18% gray. This is global exposure; local exposure and metering masks are not
implemented.

Exposure runs after TAA/STP and before Bloom, the other optional effects, and
tonemapping. SGSR2 retains its existing placement after post-processing. Bloom
therefore reacts to exposed brightness. Color Adjustments > Post Exposure remains
a separate adjustment later in color grading. HDR input is recommended to retain
highlight information before exposure.

Each camera keeps two 1x1 exposure textures. First use, re-enabling, resolution
changes, and temporal history resets immediately initialize exposure. Subsequent
frames adapt smoothly. Call `UltraAdditionalCameraData.ResetTemporalHistory()`
when cutting or teleporting an existing camera. Destroyed/unused camera histories
are retired and all histories are released with the post-processing pass.

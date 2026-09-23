shader_type spatial;
render_mode unshaded, blend_mix, depth_draw_opaque, cull_disabled;

uniform sampler2D sprite_texture : source_color, filter_linear_mipmap;
uniform float hue_shift_speed: hint_range(0.0, 5.0) = 1.0;
uniform float hue_shift_offset: hint_range(0.0, 1.0) = 0.0;

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

bool is_in_stripe(float x, float stripe_width, float stripe_spacing) {
    float cycle = stripe_width + stripe_spacing;
    float m = mod(x, cycle);
    return m < stripe_width;
}


void fragment() {
    // Sample the sprite texture
	vec4 tex_color = texture(sprite_texture, UV);
    vec3 shifted_rgb = tex_color.rgb;

    // Calculate animated hue shift based on time
    float animated_shift = mod(TIME * hue_shift_speed + hue_shift_offset, 1.0);

    // Convert to HSV, shift hue, convert back to RGB
    vec3 hsv = rgb2hsv(tex_color.rgb);
    
    float x = UV.x + UV.y/5.0 + TIME * 0.1;
    bool stripe = is_in_stripe(x, 0.03, 0.03) || is_in_stripe(x, 0.03, 0.03);
    
    if(stripe){
        hsv.x = fract(hsv.x + 0.5);
    }
    
    shifted_rgb = hsv2rgb(hsv);

    // Output with shifted color
    ALBEDO = shifted_rgb;

    // Apply transparency from sprite
    ALPHA = tex_color.a;
}
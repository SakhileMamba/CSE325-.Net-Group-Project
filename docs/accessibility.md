# Accessibility Notes

Social Net uses Bootstrap and native HTML controls as its accessibility baseline.
The current pages include the following support:

- A consistent page heading structure and descriptive navigation links.
- Visible labels or accessible names for form fields and icon-only controls.
- Validation and operation failures announced with `role="alert"`.
- Loading and empty states announced with `role="status"`.
- `aria-pressed` on like toggles.
- `aria-expanded` and `aria-controls` on controls that reveal replies.
- A visible keyboard focus outline for links and form controls.
- Decorative icons hidden from assistive technologies where appropriate.
- Native buttons, links, inputs, and text areas so features remain keyboard
  operable without custom keyboard code.

## Manual checks still required

Accessibility cannot be verified by a successful build alone. Before release,
manually check the main routes using only a keyboard, browser zoom at 200%, and a
screen reader. Also run an automated accessibility scanner and verify color
contrast, mobile reflow, error announcements, focus order, and focus behavior
after interactive updates.

These notes describe implemented support and a test plan; they do not certify
full WCAG 2.1 Level AA conformance.

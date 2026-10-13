import matplotlib.pyplot as plt
import matplotlib.patches as patches

# Set up figure with clean aspect ratio
fig, ax = plt.subplots(figsize=(11.5, 7.0), dpi=300)
ax.set_xlim(0, 115)
ax.set_ylim(0, 100)
ax.axis('off')

# Background: clean white
fig.patch.set_facecolor('#FFFFFF')

# Grayscale / Black & White palette
c_white = '#FFFFFF'
c_light_gray = '#F3F4F6'
c_mid_gray = '#E5E7EB'
c_border_gray = '#4B5563'
c_black = '#000000'
c_text_dark = '#111827'
c_arrow = '#374151'

# Helper to draw rounded rectangle box in black and white
def draw_box(x, y, w, h, title, subtitle, fill_color='#FFFFFF', border_color='#000000', text_color='#000000'):
    rect = patches.FancyBboxPatch(
        (x, y), w, h,
        boxstyle="round,pad=1.0,rounding_size=2.0",
        facecolor=fill_color,
        edgecolor=border_color,
        linewidth=1.2,
        zorder=3
    )
    ax.add_patch(rect)
    ax.text(x + w/2, y + h*0.64, title, ha='center', va='center', fontsize=10.0, fontweight='bold', color=text_color, zorder=4)
    if subtitle:
        ax.text(x + w/2, y + h*0.28, subtitle, ha='center', va='center', fontsize=7.8, color='#374151', zorder=4)

# Helper to draw container
def draw_container(x, y, w, h, label):
    rect = patches.FancyBboxPatch(
        (x, y), w, h,
        boxstyle="round,pad=1.0,rounding_size=2.5",
        facecolor='#FAFAFA',
        edgecolor='#9CA3AF',
        linewidth=1.2,
        linestyle='--',
        zorder=1
    )
    ax.add_patch(rect)
    ax.text(x + 3.5, y + h - 3.8, label, ha='left', va='center', fontsize=8.8, fontweight='bold', color='#111827', zorder=2)

# Helper to draw arrow
def draw_arrow(x1, y1, x2, y2, label=''):
    ax.annotate(
        '', xy=(x2, y2), xytext=(x1, y1),
        arrowprops=dict(facecolor=c_arrow, edgecolor=c_arrow, width=1.2, headwidth=5.5, headlength=5.5, shrink=0.04),
        zorder=5
    )
    if label:
        mid_x = (x1 + x2) / 2
        mid_y = (y1 + y2) / 2 + 2.0
        ax.text(mid_x, mid_y, label, ha='center', va='center', fontsize=7.2, color='#111827', fontweight='bold',
                bbox=dict(boxstyle='round,pad=0.25', facecolor='#FFFFFF', edgecolor='#9CA3AF', lw=0.8), zorder=6)

# Main Title Header (Black & White)
ax.text(57.5, 96.5, "CareBasket - High-Level System Architecture Overview", ha='center', va='center', fontsize=13, fontweight='bold', color=c_black)
ax.text(57.5, 93.0, "System Architecture for Healthcare E-Commerce in Bangladesh (Black and White Specification)", ha='center', va='center', fontsize=9.0, color='#4B5563')

# 1. CLIENT CONTAINER (x=2 to 31, w=29)
draw_container(2, 3, 29, 86, "1. USER INTERFACE (FRONTEND)")
draw_box(4.5, 63, 24, 15, "Customer Storefront", "HTML5, CSS3, JavaScript\nTailwind CSS (Responsive)", c_white, c_border_gray)
draw_box(4.5, 41, 24, 15, "Admin & Pharmacist Portal", "Management Web Pages\nOrder & Prescription Triage", c_light_gray, c_border_gray)
draw_box(4.5, 19, 24, 15, "Optional SPA Client", "Future React Module\n(Specialized Tools If Needed)", c_white, c_border_gray)

# 2. BACKEND API CONTAINER (x=41 to 74, w=33)
draw_container(41, 3, 33, 86, "2. BACKEND API (ASP.NET CORE)")
draw_box(43.5, 67, 28, 13, "API Endpoints & Routing", "ASP.NET Core Web API\nSwagger Docs & Rate Limiting", c_light_gray, c_border_gray)
draw_box(43.5, 48, 28, 13, "Application Logic", "Order & Prescription Rules\nInventory Reservation Checks", c_white, c_border_gray)
draw_box(43.5, 29, 28, 13, "Security & Accounts", "Identity & JWT Bearer Tokens\nRole-Based Access Control", c_light_gray, c_border_gray)
draw_box(43.5, 10, 28, 13, "Data Access (ORM)", "Entity Framework Core\nDatabase Queries & Migrations", c_white, c_border_gray)

# 3. DATA & INTEGRATION CONTAINER (x=84 to 113, w=29)
draw_container(84, 3, 29, 86, "3. DATABASE & EXTERNAL SERVICES")
draw_box(86.5, 67, 24, 13.5, "PostgreSQL Database", "Relational Tables with ACID\nTrigram Fuzzy Text Search", c_light_gray, c_border_gray)
draw_box(86.5, 48, 24, 13.5, "File Storage", "Prescriptions & Invoices\nPrivate Access Only", c_white, c_border_gray)
draw_box(86.5, 29, 24, 13.5, "Payment & SMS Gateways", "Cash on Delivery, bKash, Nagad\nSMS & Email Alerts", c_light_gray, c_border_gray)
draw_box(86.5, 10, 24, 13.5, "Future AI & Courier APIs", "AI Support Bot (Safety Guards)\nThird-Party Delivery Dispatch", c_white, c_border_gray)

# Inter-tier arrows
draw_arrow(28.5, 70.5, 43.5, 73.5, "HTTPS / JSON")
draw_arrow(28.5, 48.5, 43.5, 73.5, "HTTPS / REST")
draw_arrow(28.5, 26.5, 43.5, 73.5, "HTTPS / REST")

# Internal Backend Dispatches
draw_arrow(57.5, 67, 57.5, 61, "Dispatches")
draw_arrow(57.5, 48, 57.5, 42, "Authorizes")
draw_arrow(57.5, 29, 57.5, 23, "Queries")

# Backend to DB & Services
draw_arrow(71.5, 73.5, 86.5, 73.5, "SQL Queries")
draw_arrow(71.5, 54.5, 86.5, 54.5, "Private Files")
draw_arrow(71.5, 35.5, 86.5, 35.5, "Webhooks")
draw_arrow(71.5, 16.5, 86.5, 16.5, "Async APIs")

plt.tight_layout()
plt.savefig('architecture_diagram.png', dpi=300, bbox_inches='tight')
print("Generated clean black-and-white architecture diagram.")

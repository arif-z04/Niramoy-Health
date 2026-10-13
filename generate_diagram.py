import matplotlib.pyplot as plt
import matplotlib.patches as patches

# Set up figure with wider aspect ratio for clean spacing
fig, ax = plt.subplots(figsize=(11.5, 7.0), dpi=300)
ax.set_xlim(0, 115)
ax.set_ylim(0, 100)
ax.axis('off')

# Background
fig.patch.set_facecolor('#FFFFFF')

# Color palette
c_teal_dark = '#0F766E'
c_teal = '#0D9488'
c_teal_light = '#14B8A6'
c_navy = '#1E3A8A'
c_blue = '#2563EB'
c_blue_light = '#3B82F6'
c_data = '#047857'
c_amber = '#D97706'
c_brown = '#B45309'

# Helper to draw rounded rectangle box
def draw_box(x, y, w, h, title, subtitle, fill_color, text_color='#FFFFFF', border_color=None):
    rect = patches.FancyBboxPatch(
        (x, y), w, h,
        boxstyle="round,pad=1.0,rounding_size=2.2",
        facecolor=fill_color,
        edgecolor=border_color if border_color else fill_color,
        linewidth=1.2,
        zorder=3
    )
    ax.add_patch(rect)
    ax.text(x + w/2, y + h*0.64, title, ha='center', va='center', fontsize=10.5, fontweight='bold', color=text_color, zorder=4)
    if subtitle:
        ax.text(x + w/2, y + h*0.28, subtitle, ha='center', va='center', fontsize=8.0, color=text_color, alpha=0.92, zorder=4)

# Helper to draw container
def draw_container(x, y, w, h, label, bg='#F8FAFC', border='#CBD5E1'):
    rect = patches.FancyBboxPatch(
        (x, y), w, h,
        boxstyle="round,pad=1.0,rounding_size=2.8",
        facecolor=bg,
        edgecolor=border,
        linewidth=1.2,
        linestyle='--',
        zorder=1
    )
    ax.add_patch(rect)
    # Header tab inside container
    ax.text(x + 3.5, y + h - 3.8, label, ha='left', va='center', fontsize=9.0, fontweight='bold', color='#334155', zorder=2)

# Helper to draw arrow
def draw_arrow(x1, y1, x2, y2, label=''):
    ax.annotate(
        '', xy=(x2, y2), xytext=(x1, y1),
        arrowprops=dict(facecolor='#475569', edgecolor='#475569', width=1.4, headwidth=6, headlength=6, shrink=0.04),
        zorder=5
    )
    if label:
        mid_x = (x1 + x2) / 2
        mid_y = (y1 + y2) / 2 + 2.0
        ax.text(mid_x, mid_y, label, ha='center', va='center', fontsize=7.2, color='#1E293B', fontweight='bold',
                bbox=dict(boxstyle='round,pad=0.25', facecolor='#FFFFFF', edgecolor='#94A3B8', lw=0.8), zorder=6)

# Main Title Header
ax.text(57.5, 96.5, "CareBasket — High-Level System Architecture Overview", ha='center', va='center', fontsize=14, fontweight='bold', color='#0F172A')
ax.text(57.5, 93.0, "Modular Clean Architecture for Scalable Healthcare E-Commerce in Bangladesh", ha='center', va='center', fontsize=9.5, color='#64748B')

# 1. CLIENT CONTAINER (x=2 to 31, w=29)
draw_container(2, 3, 29, 86, "1. CLIENT PRESENTATION LAYER")
draw_box(4.5, 63, 24, 15, "Storefront Web UI", "Vite + HTML5 / Tailwind CSS\nResponsive Mobile-to-Desktop", c_teal)
draw_box(4.5, 41, 24, 15, "Admin & Clinical Portal", "Backoffice Workbench\nOrder & Rx Management", c_teal_dark)
draw_box(4.5, 19, 24, 15, "Optional SPA Client", "Future React Module\n(Specialized Workbenches)", '#134E4A')

# 2. BACKEND API CONTAINER (x=41 to 74, w=33)
draw_container(41, 3, 33, 86, "2. ASP.NET CORE BACKEND API (.NET 10)")
draw_box(43.5, 67, 28, 13, "API Gateway / Endpoints", "ASP.NET Core Web API\nSwagger OpenAPI / Rate Limiting", c_navy)
draw_box(43.5, 48, 28, 13, "Application Core & CQRS", "FluentValidation / Business Rules\nTwo-Phase Inventory Engine", c_blue)
draw_box(43.5, 29, 28, 13, "Identity & Security Guard", "JWT Bearer / Claims RBAC\nPBKDF2 Password Hashing", c_blue_light)
draw_box(43.5, 10, 28, 13, "Data Access Layer (ORM)", "Entity Framework Core 10\nAtomic Transactions & Migrations", '#1D4ED8')

# 3. DATA & INTEGRATION CONTAINER (x=84 to 113, w=29)
draw_container(84, 3, 29, 86, "3. PERSISTENCE & EXTERNAL SERVICES")
draw_box(86.5, 67, 24, 13.5, "PostgreSQL 16+ Database", "Relational Engine (ACID)\npg_trgm Fuzzy Search Indexes", c_data)
draw_box(86.5, 48, 24, 13.5, "Encrypted File Storage", "Prescriptions & Invoices\nPre-Signed Private URLs", '#059669')
draw_box(86.5, 29, 24, 13.5, "Payment & SMS Gateways", "bKash / Nagad / COD\nSMS & Email Gateways", c_amber)
draw_box(86.5, 10, 24, 13.5, "Future AI & Courier APIs", "AI Support Bot (RAG/Safety)\nCourier API Dispatch (Phase 7)", c_brown)

# Inter-tier arrows with clean spacing
# Column 1 to Column 2 (x=28.5 to 43.5, gap is 15 units!)
draw_arrow(28.5, 70.5, 43.5, 73.5, "HTTPS / JSON")
draw_arrow(28.5, 48.5, 43.5, 73.5, "HTTPS / REST")
draw_arrow(28.5, 26.5, 43.5, 73.5, "HTTPS / REST")

# Internal Backend Dispatches
draw_arrow(57.5, 67, 57.5, 61, "Dispatches")
draw_arrow(57.5, 48, 57.5, 42, "Authorizes")
draw_arrow(57.5, 29, 57.5, 23, "Queries")

# Column 2 to Column 3 (x=71.5 to 86.5, gap is 15 units!)
draw_arrow(71.5, 73.5, 86.5, 73.5, "TCP / SQL")
draw_arrow(71.5, 54.5, 86.5, 54.5, "Encrypted I/O")
draw_arrow(71.5, 35.5, 86.5, 35.5, "REST Webhooks")
draw_arrow(71.5, 16.5, 86.5, 16.5, "Async APIs")

plt.tight_layout()
plt.savefig('architecture_diagram.png', dpi=300, bbox_inches='tight')
print("Successfully regenerated architecture_diagram.png with expanded spacing")

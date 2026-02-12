const elLocale = document.getElementById("locale");
const elSeed = document.getElementById("seed");
const elRandomSeed = document.getElementById("randomSeed");
const elLikes = document.getElementById("likes");
const elLikesVal = document.getElementById("likesVal");

const btnTable = document.getElementById("btnTable");
const btnGallery = document.getElementById("btnGallery");

const content = document.getElementById("content");

let state = {
    locale: "en-US",
    seed: 1n,
    avgLikes: 3.7,
    tablePage: 1,
    pageSize: 10,
    mode: "table",
    galleryPage: 1,
    isLoading: false,
    expandedIndex: null,
    expandedTab: "lyrics"
};

function randomU64() {
    const a = BigInt(Math.floor(Math.random() * 2 ** 32));
    const b = BigInt(Math.floor(Math.random() * 2 ** 32));
    return (a << 32n) | b;
}

function debounce(fn, ms) {
    let t;
    return (...args) => {
        clearTimeout(t);
        t = setTimeout(() => fn(...args), ms);
    };
}

function fmtTime(sec) {
    sec = Math.max(0, Number(sec) || 0);
    const m = Math.floor(sec / 60);
    const s = sec % 60;
    return `${m}:${String(s).padStart(2, "0")}`;
}

async function fetchPage(page) {
    const url = `/api/songs/page?locale=${encodeURIComponent(state.locale)}&seed=${state.seed}&page=${page}&pageSize=${state.pageSize}&avgLikes=${state.avgLikes}`;
    const res = await fetch(url);
    if (!res.ok) throw new Error("API error");
    return await res.json();
}

function resetOnParamsChange() {
    state.tablePage = 1;
    state.galleryPage = 1;
    state.expandedIndex = null;
    state.expandedTab = "lyrics";
    window.scrollTo({ top: 0, behavior: "instant" });
}

const refresh = debounce(async () => {
    if (state.mode === "table") {
        const data = await fetchPage(state.tablePage);
        renderTable(data);
    } else {
        content.innerHTML = `<div class="gallery" id="gallery"></div><div id="sentinel" style="height:1px"></div>`;
        state.galleryPage = 1;
        await loadMoreGallery();
        setupInfiniteScroll();
    }
}, 120);

function setMode(mode) {
    state.mode = mode;
    btnTable.classList.toggle("active", mode === "table");
    btnGallery.classList.toggle("active", mode === "gallery");
    resetOnParamsChange();
    refresh();
}

function bindControls() {
    elLocale.value = state.locale;
    elSeed.value = state.seed.toString();
    elLikes.value = String(state.avgLikes);
    elLikesVal.textContent = Number(state.avgLikes).toFixed(1);

    elLocale.addEventListener("change", () => {
        state.locale = elLocale.value;
        resetOnParamsChange();
        refresh();
    });

    elSeed.addEventListener("input", () => {
        try {
            const v = BigInt(elSeed.value || "0");
            state.seed = v < 0n ? 0n : v;
            resetOnParamsChange();
            refresh();
        } catch { }
    });

    elRandomSeed.addEventListener("click", () => {
        state.seed = randomU64();
        elSeed.value = state.seed.toString();
        resetOnParamsChange();
        refresh();
    });

    elLikes.addEventListener("input", () => {
        const v = Number(elLikes.value);
        state.avgLikes = Number.isFinite(v) ? Math.min(10, Math.max(0, v)) : 0;
        elLikesVal.textContent = Number(state.avgLikes).toFixed(1);
        refresh();
    });

    btnTable.addEventListener("click", () => setMode("table"));
    btnGallery.addEventListener("click", () => setMode("gallery"));
}

function renderPager() {
    const p = state.tablePage;
    const pages = [p - 1, p, p + 1].filter(x => x >= 1);

    return `
    <div class="pager">
      <button class="page-btn" data-act="prev">«</button>
      ${pages.map(x => `<button class="page-btn ${x === p ? "active" : ""}" data-page="${x}">${x}</button>`).join("")}
      <button class="page-btn" data-act="next">»</button>
    </div>
  `;
}

function renderTable(data) {
    const rows = data.items.map(item => {
        const isExpanded = state.expandedIndex === item.index;
        const arrow = isExpanded ? "▴" : "▾";

        const details = isExpanded ? `
      <tr class="expand-row">
        <td colspan="6">
          ${renderDetails(item)}
        </td>
      </tr>
    ` : "";

        return `
      <tr class="data-row" data-idx="${item.index}">
        <td class="col-arrow">${arrow}</td>
        <td class="col-num">${item.index}</td>
        <td>
          <div class="song-title">${item.title}</div>
        </td>
        <td>${item.artist}</td>
        <td>${item.album}</td>
        <td>${item.genre}</td>
      </tr>
      ${details}
    `;
    }).join("");

    content.innerHTML = `
    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th class="col-arrow"></th>
            <th class="col-num">#</th>
            <th>Song</th>
            <th>Artist</th>
            <th>Album</th>
            <th>Genre</th>
          </tr>
        </thead>
        <tbody>${rows}</tbody>
      </table>
    </div>
    ${renderPager()}
  `;

    content.querySelectorAll("tbody tr.data-row").forEach(tr => {
        tr.addEventListener("click", () => {
            const idx = Number(tr.getAttribute("data-idx"));
            state.expandedIndex = (state.expandedIndex === idx) ? null : idx;
            state.expandedTab = "lyrics";
            renderTable(data);
            if (state.expandedIndex) {
                const el = content.querySelector(`tbody tr.data-row[data-idx="${idx}"]`);
                if (el) el.scrollIntoView({ block: "nearest" });
            }
        });
    });

    // pager
    content.querySelectorAll(".page-btn[data-page]").forEach(btn => {
        btn.addEventListener("click", () => {
            state.tablePage = Number(btn.getAttribute("data-page"));
            state.expandedIndex = null;
            refresh();
        });
    });
    content.querySelectorAll(".page-btn[data-act]").forEach(btn => {
        btn.addEventListener("click", () => {
            const act = btn.getAttribute("data-act");
            if (act === "prev") state.tablePage = Math.max(1, state.tablePage - 1);
            if (act === "next") state.tablePage = state.tablePage + 1;
            state.expandedIndex = null;
            refresh();
        });
    });

    content.querySelectorAll("[data-tab]").forEach(btn => {
        btn.addEventListener("click", () => {
            state.expandedTab = btn.getAttribute("data-tab");
            renderTable(data);
        });
    });
}

function renderDetails(item) {
    const isLyrics = state.expandedTab === "lyrics";
    const isAbout = state.expandedTab === "about";

    const lyricsHtml = escapeHtml(item.lyrics || "").replace(/\n/g, "<br/>");
    const aboutText = `
Likes: ${item.likes}
Review: ${item.review || ""}
`;

    return `
    <div class="details">
      <img class="cover" src="${item.coverDataUrl}" alt="cover"/>
      <div class="details-main">
        <div class="h1">${escapeHtml(item.title)}</div>
        <div class="meta">
          from <b>${escapeHtml(item.album)}</b> by <b>${escapeHtml(item.artist)}</b>
          <span class="like-chip" style="margin-left:8px;">❤ ${item.likes}</span>
          <span class="like-chip" style="margin-left:6px;">${fmtTime(item.durationSec)}</span>
        </div>
        <div class="meta2">${escapeHtml(item.label)} • ${item.year}</div>

        <div class="player-row">
          <audio controls src="${item.audioUrl}"></audio>
        </div>

        <div class="tabs">
          <button class="tab ${isLyrics ? "active" : ""}" data-tab="lyrics">Lyrics</button>
          <button class="tab ${isAbout ? "active" : ""}" data-tab="about">About</button>
        </div>

        <div class="lyrics">
          ${isLyrics ? lyricsHtml : escapeHtml(aboutText).replace(/\n/g, "<br/>")}
        </div>
      </div>
    </div>
  `;
}

function escapeHtml(s) {
    return String(s ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

async function loadMoreGallery() {
    if (state.isLoading) return;
    state.isLoading = true;

    const data = await fetchPage(state.galleryPage);
    const gallery = document.getElementById("gallery");

    data.items.forEach(item => {
        const div = document.createElement("div");
        div.className = "card";
        div.innerHTML = `
      <img src="${item.coverDataUrl}" alt="cover"/>
      <div class="t">${escapeHtml(item.title)}</div>
      <div class="s">${escapeHtml(item.artist)}</div>
      <div class="s">${escapeHtml(item.album)} • ${escapeHtml(item.genre)}</div>
      <div class="s">❤ ${item.likes} • ${fmtTime(item.durationSec)}</div>
      <audio controls src="${item.audioUrl}" style="width:100%; margin-top:8px;"></audio>
    `;
        gallery.appendChild(div);
    });

    state.galleryPage += 1;
    state.isLoading = false;
}

function setupInfiniteScroll() {
    const sentinel = document.getElementById("sentinel");
    const io = new IntersectionObserver(async (entries) => {
        if (entries.some(e => e.isIntersecting)) {
            await loadMoreGallery();
        }
    });
    io.observe(sentinel);
}

async function init() {
    bindControls();
    await refresh();
}

init().catch(err => {
    content.innerHTML = `<div style="color:red;">Error: ${err.message}</div>`;
});

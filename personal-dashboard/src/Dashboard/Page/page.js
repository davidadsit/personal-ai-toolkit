(() => {
  const storage = (area) => ({
    get(key) { try { return window[area].getItem(key); } catch { return null; } },
    set(key, value) { try { window[area].setItem(key, value); } catch { } },
  });
  const session = storage("sessionStorage");
  const local = storage("localStorage");
  const readJson = (text, fallback) => { try { return JSON.parse(text) ?? fallback; } catch { return fallback; } };

  const body = document.body;
  const work = document.getElementById("work");
  const panels = document.getElementById("panels");

  function saveScroll() {
    session.set("scroll", JSON.stringify({ window: window.scrollY, work: work.scrollTop, panels: panels.scrollTop }));
  }

  function restoreScroll() {
    const saved = readJson(session.get("scroll"), null);
    if (!saved) return;
    window.scrollTo(0, saved.window);
    work.scrollTop = saved.work;
    panels.scrollTop = saved.panels;
  }

  function selectedProject() {
    const match = location.hash.match(/^#p=(\d+)$/);
    return match ? match[1] : null;
  }

  function applyFilter() {
    const selected = selectedProject();
    document.querySelectorAll(".project").forEach((button) => {
      const isAll = button.hasAttribute("data-all");
      button.classList.toggle("selected", isAll ? selected === null : button.dataset.project === selected);
    });
    document.querySelectorAll(".lane").forEach((lane) => {
      let visible = 0;
      lane.querySelectorAll(".card").forEach((card) => {
        card.hidden = selected !== null && !card.dataset.projects.split(" ").includes(selected);
        if (!card.hidden) visible++;
      });
      lane.querySelector(".count").textContent = visible;
    });
    filterOpenItems(selected);
  }

  function filterOpenItems(selected) {
    const items = document.querySelectorAll(".open-item");
    if (items.length === 0) return;
    let visible = 0;
    items.forEach((item) => {
      item.hidden = selected !== null && item.dataset.projects !== selected;
      if (!item.hidden) visible++;
    });
    document.getElementById("no-open-items-for-project").hidden = visible > 0;
  }

  function select(project) {
    const url = project === null ? location.pathname + location.search : "#p=" + project;
    try { history.replaceState(null, "", url); } catch { location.hash = project === null ? "" : "p=" + project; }
    applyFilter();
  }

  function applyLanes() {
    document.querySelectorAll(".lane[data-collapsible]").forEach((lane) => {
      lane.classList.toggle("collapsed", session.get("lane:" + lane.dataset.lane) !== "open");
    });
  }

  function toggleLane(lane) {
    const opening = lane.classList.contains("collapsed");
    session.set("lane:" + lane.dataset.lane, opening ? "open" : "closed");
    applyLanes();
  }

  function restoreCompleted() {
    const completed = document.getElementById("completed");
    if (!completed) return;
    completed.open = session.get("completed") === "open";
    completed.addEventListener("toggle", () => session.set("completed", completed.open ? "open" : "closed"));
  }

  function flashOnce(meetingId) {
    const key = "flashed:" + meetingId;
    if (session.get(key)) return;
    session.set(key, "yes");
    body.classList.remove("flash");
    void body.offsetWidth;
    body.classList.add("flash");
  }

  function tickMeetings() {
    const meetings = document.querySelectorAll(".meeting");
    if (meetings.length === 0) return;
    const now = Date.now();
    let remaining = 0;
    meetings.forEach((meeting) => {
      const start = Number(meeting.dataset.start);
      const ended = now >= Number(meeting.dataset.end);
      const imminent = !ended && now >= start - 60000 && now < start;
      meeting.hidden = ended;
      meeting.classList.toggle("imminent", imminent);
      if (!ended) remaining++;
      if (imminent) flashOnce(meeting.dataset.id);
    });
    document.getElementById("no-more-meetings").hidden = remaining > 0;
  }

  const addressedKeys = () => new Set(readJson(local.get("addressed"), []));

  function applyAddressed() {
    const emails = document.querySelectorAll(".email");
    if (emails.length === 0) return;
    const addressed = addressedKeys();
    let visible = 0;
    emails.forEach((email) => {
      email.hidden = addressed.has(email.dataset.key);
      if (!email.hidden) visible++;
    });
    document.getElementById("nothing-else").hidden = visible > 0;
  }

  function markAddressed(email) {
    const addressed = addressedKeys();
    addressed.add(email.dataset.key);
    local.set("addressed", JSON.stringify([...addressed]));
    applyAddressed();
  }

  async function archive(email, button) {
    button.disabled = true;
    try {
      const response = await fetch("/archive", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ thread: email.dataset.thread }),
      });
      if (!response.ok) throw new Error(String(response.status));
      markAddressed(email);
    } catch {
      button.textContent = "Archive failed";
      button.disabled = false;
    }
  }

  document.addEventListener("click", (event) => {
    const target = event.target;
    const project = target.closest(".project");
    if (project) {
      const chosen = project.hasAttribute("data-all") || project.dataset.project === selectedProject() ? null : project.dataset.project;
      select(chosen);
      return;
    }
    const toggle = target.closest(".lane-toggle");
    if (toggle) { toggleLane(toggle.closest(".lane")); return; }
    if (target.closest("a")) return;
    const card = target.closest(".card");
    if (card) { select(selectedProject() === null ? card.dataset.projects.split(" ")[0] : null); return; }
    const email = target.closest(".email");
    if (!email) return;
    if (target.closest(".addressed")) markAddressed(email);
    const archiveButton = target.closest(".archive");
    if (archiveButton) archive(email, archiveButton);
  });

  if (location.protocol === "file:") body.classList.add("from-file");
  window.addEventListener("hashchange", applyFilter);
  [window, work, panels].forEach((scroller) => scroller.addEventListener("scroll", saveScroll, { passive: true }));

  applyLanes();
  applyFilter();
  restoreCompleted();
  applyAddressed();
  tickMeetings();
  restoreScroll();
  setInterval(tickMeetings, 1000);
  setTimeout(() => { saveScroll(); location.reload(); }, Number(body.dataset.reloadSeconds) * 1000);
})();

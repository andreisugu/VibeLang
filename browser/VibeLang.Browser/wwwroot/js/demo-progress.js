// Demo-only progress store: completed lessons + XP live in localStorage.
(function () {
    const key = 'vibelang-demo-progress';
    window.vibelangProgress = {
        get: function () {
            try {
                return JSON.parse(localStorage.getItem(key)) || { lessons: [], xp: 0 };
            } catch (e) {
                return { lessons: [], xp: 0 };
            }
        },
        complete: function (lessonId, score) {
            const p = this.get();
            if (!p.lessons.includes(lessonId)) p.lessons.push(lessonId);
            p.xp = (p.xp || 0) + (score || 0);
            p.lastLessonId = lessonId;
            p.lastXpAdded = score || 0;
            localStorage.setItem(key, JSON.stringify(p));
            return p;
        },
        reset: function () { localStorage.removeItem(key); }
    };
})();

/**
 * LessonParser - Handles interactive language lessons and quizzes
 * Adapted from TerminaLingo (Java) to VibeLang (Web)
 */
class LessonParser {
    constructor(lessonId) {
        this.lessonId = lessonId;
        this.lesson = null;
        this.remainingTests = [];
        this.totalTestCount = 0;
        this.correctStreak = 0;
        this.currentTest = null;
        this.isWaitingForNext = false;

        // Matching Game State (Tip 3)
        this.matchingLeftSelected = null;
        this.matchingRightSelected = null;
        this.matchedPairs = new Set();
        this.wordBankButtons = [];

        // UI Elements
        this.elements = {
            container: document.querySelector('.card'),
            progress: document.getElementById('quiz-progress'),
            questionText: document.getElementById('question-text'),
            questionHint: document.getElementById('question-hint'),
            content: document.getElementById('question-content'),
            feedbackArea: document.getElementById('feedback-area'),
            feedbackMsg: document.getElementById('feedback-message'),
            btnCheck: document.getElementById('btn-check'),
            btnNext: document.getElementById('btn-next'),
            resultsView: document.getElementById('results-view'),
            finalScore: document.getElementById('final-score'),
            xpEarned: document.getElementById('xp-earned')
        };

        this.init();
    }

    async init() {
        try {
            const response = await fetch(`/home/getlessondata/${this.lessonId}`);
            if (!response.ok) throw new Error('Failed to load lesson data');
            
            this.lesson = await response.json();
            this.remainingTests = [...this.lesson.teste];
            this.totalTestCount = this.remainingTests.length;
            
            this.setupEventListeners();
            this.renderNext();
        } catch (error) {
            console.error('Initialization error:', error);
            this.elements.questionText.innerText = 'Error loading lesson.';
        }
    }

    setupEventListeners() {
        this.elements.btnCheck.onclick = () => this.checkAnswer();
        this.elements.btnNext.onclick = () => this.renderNext();

        // Global key listener
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                if (this.isWaitingForNext) {
                    this.renderNext();
                } else if (!this.elements.btnCheck.disabled) {
                    this.checkAnswer();
                }
            }
            // Number shortcuts for Word Bank (Tip 1)
            else if (this.currentTest && this.currentTest.tip === 1 && !this.isWaitingForNext) {
                if (e.key === 'Backspace') {
                    const builder = document.getElementById('sentence-builder');
                    if (builder && builder.lastElementChild) {
                        builder.lastElementChild.click();
                        e.preventDefault();
                    }
                } else {
                    const num = parseInt(e.key);
                    if (num >= 1 && num <= 9) {
                        const index = num - 1;
                        if (this.wordBankButtons[index] && !this.wordBankButtons[index].disabled) {
                            this.wordBankButtons[index].click();
                        }
                    }
                }
            }
        });
    }

    renderNext() {
        this.isWaitingForNext = false;
        this.elements.feedbackArea.classList.add('d-none');
        this.elements.btnNext.classList.add('d-none');
        this.elements.btnCheck.classList.remove('d-none');
        this.elements.btnCheck.disabled = false;
        this.elements.content.innerHTML = '';
        this.elements.questionHint.innerText = '';

        if (this.remainingTests.length === 0) {
            this.finishLesson();
            return;
        }

        this.currentTest = this.remainingTests.shift();
        this.updateProgress();

        switch (this.currentTest.tip) {
            case 1: this.renderTip1(); break; // RO -> EN with hints
            case 2: this.renderTip2(); break; // EN -> RO
            case 3: this.renderTip3(); break; // Matching
            case 4: this.renderTip4(); break; // Context/Info
            default: this.renderNext(); break; // Skip unknown
        }
    }

    updateProgress() {
        const completed = this.totalTestCount - this.remainingTests.length - 1;
        const percent = Math.max(0, (completed / this.totalTestCount) * 100);
        this.elements.progress.style.width = `${percent}%`;
    }

    /** RO -> EN with word hints */
    renderTip1() {
        this.elements.questionText.innerText = 'Translate the following sentence:';
        this.elements.questionHint.innerText = this.currentTest.propozitie;

        const container = document.createElement('div');
        
        // Input display area
        const inputDisplay = document.createElement('div');
        inputDisplay.className = 'form-control min-vh-10 mb-3 d-flex flex-wrap gap-2 align-items-center';
        inputDisplay.style.minHeight = '60px';
        inputDisplay.id = 'sentence-builder';
        container.appendChild(inputDisplay);

        // Word bank container
        const wordBank = document.createElement('div');
        wordBank.className = 'd-flex flex-wrap justify-content-center';
        wordBank.id = 'word-bank';
        
        this.wordBankButtons = [];
        const words = this.normalizeAndSplit(this.currentTest.raspunsCorrect || this.currentTest.raspunsCorect);
        
        const updateWordBankUI = () => {
            wordBank.innerHTML = '';
            this.wordBankButtons.forEach((btn, index) => {
                // Clear old labels
                const oldLabel = btn.querySelector('.badge');
                if (oldLabel) oldLabel.remove();

                // Add new number shortcut label (1-9)
                if (index < 9) {
                    const label = document.createElement('small');
                    label.className = 'position-absolute top-0 start-100 translate-middle badge rounded-pill bg-light text-dark border';
                    label.style.fontSize = '0.6rem';
                    label.innerText = index + 1;
                    btn.appendChild(label);
                }
                wordBank.appendChild(btn);
            });
        };

        this.shuffle(words).forEach((word) => {
            const btn = document.createElement('button');
            btn.className = 'btn btn-outline-primary rounded-pill word-bank-btn position-relative';
            btn.innerHTML = `<span>${word}</span>`;
            
            btn.onclick = () => {
                const clone = document.createElement('button');
                clone.className = 'btn btn-primary rounded-pill word-bank-btn';
                clone.innerText = word;
                clone.onclick = () => {
                    clone.remove();
                    this.wordBankButtons.push(btn);
                    updateWordBankUI();
                };
                inputDisplay.appendChild(clone);
                
                // Physically remove from available buttons
                this.wordBankButtons = this.wordBankButtons.filter(b => b !== btn);
                updateWordBankUI();
            };
            
            this.wordBankButtons.push(btn);
        });

        updateWordBankUI();
        container.appendChild(wordBank);
        this.elements.content.appendChild(container);
    }

    /** EN -> RO Free text */
    renderTip2() {
        this.elements.questionText.innerText = 'Translate this sentence:';
        this.elements.questionHint.innerText = this.currentTest.propozitie;

        const input = document.createElement('input');
        input.type = 'text';
        input.className = 'form-control form-control-lg text-center rounded-pill';
        input.placeholder = 'Type translation here...';
        input.id = 'text-answer';
        input.autocomplete = 'off';
        
        this.elements.content.appendChild(input);
        setTimeout(() => input.focus(), 100);
    }

    /** Matching Pairs */
    renderTip3() {
        this.elements.questionText.innerText = 'Match the pairs:';
        this.matchedPairs = new Set();
        this.matchingLeftSelected = null;
        this.matchingRightSelected = null;

        const grid = document.createElement('div');
        grid.className = 'row g-3';

        const leftCol = document.createElement('div');
        leftCol.className = 'col-6 d-flex flex-column gap-2';
        
        const rightCol = document.createElement('div');
        rightCol.className = 'col-6 d-flex flex-column gap-2';

        const leftWords = this.shuffle([...this.currentTest.leftWords]);
        const rightWords = this.shuffle([...this.currentTest.rightWords]);

        leftWords.forEach(word => {
            const div = document.createElement('div');
            div.className = 'matching-word';
            div.innerText = word;
            div.dataset.type = 'left';
            div.onclick = () => this.handleMatchingClick(div);
            leftCol.appendChild(div);
        });

        rightWords.forEach(word => {
            const div = document.createElement('div');
            div.className = 'matching-word';
            div.innerText = word;
            div.dataset.type = 'right';
            div.onclick = () => this.handleMatchingClick(div);
            rightCol.appendChild(div);
        });

        grid.appendChild(leftCol);
        grid.appendChild(rightCol);
        this.elements.content.appendChild(grid);
        
        // Disable check button, matching is self-checking
        this.elements.btnCheck.disabled = true;
    }

    handleMatchingClick(element) {
        const type = element.dataset.type;
        const text = element.innerText;

        // Clear previous selection in same column
        const column = type === 'left' ? 'matchingLeftSelected' : 'matchingRightSelected';
        if (this[column]) this[column].classList.remove('selected');

        this[column] = element;
        element.classList.add('selected');

        if (this.matchingLeftSelected && this.matchingRightSelected) {
            this.checkMatchingPair();
        }
    }

    checkMatchingPair() {
        const leftText = this.matchingLeftSelected.innerText;
        const rightText = this.matchingRightSelected.innerText;
        
        // Find if this is a valid pair in original data
        const leftIdx = this.currentTest.leftWords.indexOf(leftText);
        const rightIdx = this.currentTest.rightWords.indexOf(rightText);
        
        if (leftIdx !== -1 && leftIdx === this.currentTest.rightWords.indexOf(this.currentTest.rightWords[leftIdx]) && this.currentTest.rightWords[leftIdx] === rightText) {
            // Correct
            this.matchingLeftSelected.classList.add('correct');
            this.matchingRightSelected.classList.add('correct');
            this.matchedPairs.add(leftText);
            
            if (this.matchedPairs.size === this.currentTest.leftWords.length) {
                this.showFeedback(true);
            }
        } else {
            // Incorrect
            const l = this.matchingLeftSelected;
            const r = this.matchingRightSelected;
            l.classList.add('incorrect');
            r.classList.add('incorrect');
            setTimeout(() => {
                l.classList.remove('incorrect', 'selected');
                r.classList.remove('incorrect', 'selected');
            }, 500);
        }

        this.matchingLeftSelected = null;
        this.matchingRightSelected = null;
    }

    /** Context/Info */
    renderTip4() {
        this.elements.questionText.innerText = 'Information:';
        this.elements.content.innerHTML = `<div class="p-3 bg-light rounded italic">${this.currentTest.propozitie}</div>`;
        this.elements.btnCheck.innerText = 'Continue';
    }

    checkAnswer() {
        if (this.currentTest.tip === 4) {
            this.renderNext();
            return;
        }

        let isCorrect = false;
        let userAnswer = '';
        const correctAnswer = this.currentTest.raspunsCorrect || this.currentTest.raspunsCorect;

        if (this.currentTest.tip === 1) {
            userAnswer = Array.from(document.getElementById('sentence-builder').children)
                .map(btn => btn.innerText).join(' ');
            isCorrect = this.answerCompare(userAnswer, correctAnswer);
        } 
        else if (this.currentTest.tip === 2) {
            userAnswer = document.getElementById('text-answer').value;
            isCorrect = this.answerCompare(userAnswer, correctAnswer);
        }

        if (isCorrect) {
            this.correctStreak++;
            this.showFeedback(true);
        } else {
            this.correctStreak--;
            this.showFeedback(false, correctAnswer);
            // Put failed test back at the end
            this.remainingTests.push(this.currentTest);
        }
    }

    showFeedback(isCorrect, correctAnswer = '') {
        this.isWaitingForNext = true;
        this.elements.feedbackArea.classList.remove('d-none');
        this.elements.btnCheck.classList.add('d-none');
        this.elements.btnNext.classList.remove('d-none');

        if (isCorrect) {
            this.elements.feedbackMsg.className = 'alert alert-success p-3 mb-0';
            this.elements.feedbackMsg.innerHTML = '<strong>Correct!</strong> Keep it up.';
        } else {
            this.elements.feedbackMsg.className = 'alert alert-danger p-3 mb-0';
            this.elements.feedbackMsg.innerHTML = `<strong>Incorrect.</strong> The correct answer was: <br><em>${correctAnswer}</em>`;
        }
    }

    async finishLesson() {
        this.elements.progress.style.width = '100%';
        
        // Logic from Java: score = 50 + (streak / total * 50)
        let streak = Math.max(0, this.correctStreak);
        const score = Math.min(100, Math.floor(50 + (streak / this.totalTestCount) * 50));

        // Submit to server
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        try {
            const response = await fetch('/home/submitresult', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({
                    lessonId: this.lessonId,
                    score: score
                })
            });

            const result = await response.json();
            
            // Show results UI
            document.querySelector('.card:not(#results-view)').classList.add('d-none');
            this.elements.resultsView.classList.remove('d-none');
            this.elements.finalScore.innerText = score;
            this.elements.xpEarned.innerText = `+${result.xpAdded || score}`;

        } catch (error) {
            console.error('Error submitting result:', error);
            alert('Lesson finished! But failed to save progress.');
        }
    }

    /** Normalization Logic (Matching Java impl) */
    normalizeString(text) {
        if (!text) return "";
        let n = text.toLowerCase().trim();
        n = n.replace(/ă/g, 'a').replace(/î/g, 'i').replace(/ș/g, 's')
             .replace(/ț/g, 't').replace(/â/g, 'a');
        return n;
    }

    normalizeAndSplit(text) {
        const normalized = this.normalizeString(text);
        return normalized.split(/[\s,.?!]+/).filter(w => w.length > 0);
    }

    answerCompare(guess, correct) {
        const g = this.normalizeAndSplit(guess);
        const c = this.normalizeAndSplit(correct);
        if (g.length !== c.length) return false;
        return g.every((word, i) => word === c[i]);
    }

    shuffle(array) {
        let currentIndex = array.length, randomIndex;
        while (currentIndex != 0) {
            randomIndex = Math.floor(Math.random() * currentIndex);
            currentIndex--;
            [array[currentIndex], array[randomIndex]] = [array[randomIndex], array[currentIndex]];
        }
        return array;
    }
}

// Start when document loaded
document.addEventListener('DOMContentLoaded', () => {
    if (window.lessonId) {
        new LessonParser(window.lessonId);
    }
});

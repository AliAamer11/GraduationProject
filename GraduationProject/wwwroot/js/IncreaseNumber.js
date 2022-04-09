let nums = document.querySelectorAll(".cardBox .card-a .numbers");
let started = false //function started ? no
//nums.forEach((num) => startCount(num));


if (!started) {
    nums.forEach((num) => startCount(num));
}
started = true;


function startCount(el) {
    let goal = el.dataset.goal;
    let count = setInterval(() => {
        el.textContent++;
        if (el.textContent == goal) {
            clearInterval(count);
        }
    }, 2000/goal);
}

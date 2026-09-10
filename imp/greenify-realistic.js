import jsonfile from "jsonfile";
import moment from "moment";
import simpleGit from "simple-git";
import random from "random";

const path = "./data.json";

// Light Realistic Pattern - 1-3 commits/day, weekdays heavier
const DAYS_TO_FILL = 180;
const MIN_COMMITS = 1;
const MAX_COMMITS = 3;

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

const isWeekend = (daysAgo) => {
  const day = moment().subtract(daysAgo, "d").day();
  return day === 0 || day === 6;
};

const makeCommitsForDay = async (daysAgo, numCommits) => {
  for (let i = 0; i < numCommits; i++) {
    const weekend = isWeekend(daysAgo);
    const hour = weekend ? random.int(10, 16) : random.int(9, 20);
    const minute = random.int(0, 59);
    const commitDate = moment().subtract(daysAgo, "d").hour(hour).minute(minute).format();

    const data = { date: commitDate, day: daysAgo, commit: i + 1 };

    await new Promise((resolve) => {
      jsonfile.writeFile(path, data, () => {
        simpleGit()
          .add([path])
          .commit(commitDate, { "--date": commitDate }, (err) => {
            if (err) console.error("Error:", err);
            resolve();
          });
      });
    });
    await sleep(10);
  }

  const dateStr = moment().subtract(daysAgo, "d").format("YYYY-MM-DD");
  const type = isWeekend(daysAgo) ? "(weekend)" : "(weekday)";
  console.log(`✓ Day ${daysAgo} (${dateStr}) ${type}: ${numCommits} commits`);
};

const makeCommits = async (n) => {
  if (n === 0) {
    console.log("\n🎉 Light realistic commits done! Pushing...");
    return simpleGit().push();
  }

  const weekend = isWeekend(n);
  const commitsToday = weekend
    ? random.int(0, 2)  // 0-1 commits on weekends
    : random.int(MIN_COMMITS, MAX_COMMITS);  // 1-3 on weekdays

  if (commitsToday > 0) {
    await makeCommitsForDay(n, commitsToday);
  }

  await sleep(40);

  if (n % 30 === 0) {
    console.log(`📊 Progress: ${Math.round(((180 - n) / 180) * 100)}%`);
  }

  return makeCommits(n - 1);
};

console.log("🌱 goGreen - Light Realistic Mode");
console.log("📅 180 days | Weekdays: 1-3 commits | Weekends: 0-1 commits\n");

makeCommits(DAYS_TO_FILL)
  .then(() => console.log("\n✅ Done! Your graph is now realistically green!"))
  .catch((err) => console.error("❌ Error:", err));
